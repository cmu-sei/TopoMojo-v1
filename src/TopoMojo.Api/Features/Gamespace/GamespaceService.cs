// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Api.Data.Abstractions;
using TopoMojo.Api.Data.Extensions;
using TopoMojo.Api.Exceptions;
using TopoMojo.Api.Extensions;
using TopoMojo.Hypervisor;
using TopoMojo.Api.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace TopoMojo.Api.Services
{
    public class GamespaceService : _Service
    {
        public GamespaceService(
            ILogger<GamespaceService> logger,
            IMapper mapper,
            CoreOptions options,
            IHypervisorService podService,
            IGamespaceStore gamespaceStore,
            IWorkspaceStore workspaceStore,
            ILockService lockService,
            IDistributedCache distributedCache

        ) : base (logger, mapper, options)
        {
            _pod = podService;
            _store = gamespaceStore;
            _workspaceStore = workspaceStore;
            _locker = lockService;
            _random = new Random();
            _distCache = distributedCache;
        }

        private readonly IHypervisorService _pod;
        private readonly IGamespaceStore _store;
        private readonly IWorkspaceStore _workspaceStore;
        private readonly ILockService _locker;
        private readonly Random _random;
        private readonly IDistributedCache _distCache;

        public async Task<Gamespace[]> List(GamespaceSearch search, string subjectId, bool sudo, CancellationToken ct = default(CancellationToken))
        {
            var query =  (sudo && search.WantsAll)
                ? _store.List(search.Term)
                : _store.ListByUser(subjectId)
            ;

            if (search.WantsActive)
                query = query.Where(g => g.EndTime == DateTime.MinValue);

            query = query.OrderBy(g => g.WhenCreated);

            if (search.Skip > 0)
                query = query.Skip(search.Skip);

            if (search.Take > 0)
                query = query.Take(search.Take);

            return Mapper.Map<Gamespace[]>(
                await query.ToArrayAsync()
            );
        }

        public async Task<GameState> Preview(string resourceId)
        {
            var ctx = await LoadContext(null, resourceId);

            // if (!ctx.WorkspaceExists)
            //     throw new ResourceNotFound();

            // if (!ctx.IsValidAudience)
            //     throw new InvalidClientAudience();

            // if (ctx.UserExists)
            // {
            //     ctx.Gamespace = await _store
            //         .LoadActiveByContext(ctx.Workspace.Id, User.Id);
            // }

            return new GameState
            {
                Name = ctx.Workspace.Name,

                Markdown = (await LoadMarkdown(ctx.Workspace.Id)).Split("<!-- cut -->").First()
                    ?? $"# {ctx.Workspace.Name}"
            };
        }

        public async Task<GameState> Register(GamespaceRegistration request, User actor)
        {
            var gamespace = await _Register(request, actor);

            if (request.StartGamespace)
                await Deploy(gamespace);

            return await LoadState(gamespace, request.AllowPreview);
        }

        private async Task<TopoMojo.Api.Data.Gamespace> _Register(GamespaceRegistration request, User actor)
        {
            string playerId = request.Players.FirstOrDefault()?.SubjectId ?? actor.Id;

            var gamespace = await _store.LoadActiveByContext(
                request.ResourceId,
                playerId
            );

            if (gamespace is Data.Gamespace)
                return gamespace;

            if (! await _store.IsBelowGamespaceLimit(actor.Id, actor.GamespaceLimit))
                throw new ClientGamespaceLimitReached();

            string lockKey = $"{playerId}{request.ResourceId}";

            var ctx = await LoadContext(request);

            if (! await _locker.Lock(lockKey))
                throw new ResourceIsLocked();

            try
            {
                await Create(ctx, actor);
            }
            finally
            {
                await _locker.Unlock(lockKey);
            }

            return ctx.Gamespace;
        }

        public async Task<GameState> Load(string id)
        {
            var ctx = await LoadContext(id);

            return await LoadState(ctx.Gamespace);
        }

        public async Task<GameState> Start(string id)
        {
            var ctx = await LoadContext(id);

            if (ctx.WorkspaceExists && !ctx.Gamespace.IsExpired)
                await Deploy(ctx.Gamespace);

            return await LoadState(ctx.Gamespace);
        }

        public async Task<GameState> Stop(string id)
        {
            var ctx = await LoadContext(id);

            await _pod.DeleteAll(id);

            return await LoadState(ctx.Gamespace);
        }

        public async Task<GameState> Complete(string id)
        {
            var ctx = await LoadContext(id);

            if (ctx.Gamespace.IsActive)
            {
                ctx.Gamespace.EndTime = DateTime.UtcNow;

                await _store.Update(ctx.Gamespace);
            }

            await _pod.DeleteAll(id);

            return await LoadState(ctx.Gamespace);
        }

        private async Task Create(RegistrationContext ctx, User actor)
        {

            var ts = DateTime.UtcNow;

            var gamespace = new Data.Gamespace
            {
                Name = ctx.Workspace.Name,
                Workspace = ctx.Workspace,
                ManagerId = actor.Id,
                ManagerName = actor.Name,
                AllowReset = ctx.Request.AllowReset,
                CleanupGraceMinutes = actor.GamespaceCleanupGraceMinutes,
                WhenCreated = ts,
                ExpirationTime = ctx.Request.ResolveExpiration(ts, actor.GamespaceMaxMinutes)
            };

            foreach (var player in ctx.Request.Players)
            {
                gamespace.Players.Add(
                    new Data.Player
                    {
                        SubjectId = player.SubjectId,
                        SubjectName = player.SubjectName
                    }
                );
            }

            // clone challenge
            var spec = JsonSerializer.Deserialize<ChallengeSpec>(ctx.Workspace.Challenge ?? "{}", jsonOptions);

            //resolve transforms
            ResolveTransforms(spec);

            // TODO: if customize-script, run and update transforms

            // select variant, adjusting from 1-based to 0-based index
            int v = ctx.Request.Variant > 0
                ? Math.Min(ctx.Request.Variant, spec.Variants.Count) - 1
                : _random.Next(spec.Variants.Count);

            spec.Challenge = spec.Variants
                .Skip(v).Take(1)
                .FirstOrDefault();

            // initialize selected challenge
            spec.Challenge.SetQuestionWeights();

            spec.MaxPoints = ctx.Request.Points;

            spec.MaxAttempts = ctx.Request.MaxAttempts;

            spec.Variants = null;

            gamespace.Challenge = JsonSerializer.Serialize(spec, jsonOptions);

            // apply transforms
            foreach (var kvp in spec.Transforms)
                gamespace.Challenge = gamespace.Challenge.Replace($"##{kvp.Key}##", kvp.Value);

            await _store.Create(gamespace);

            ctx.Gamespace = gamespace;
        }

        private void ResolveTransforms(ChallengeSpec spec)
        {
            foreach(var kvp in spec.Transforms.ToArray())
            {
                kvp.Value = ResolveRandom(kvp.Value);

                // insert `key_index: value` for any multi-token values (i.e. list-resolver)
                var tokens =  kvp.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 1)
                {
                    int i = 0;
                    foreach (string token in tokens)
                    {
                        spec.Transforms.Add(new StringKeyValue
                        {
                            Key = $"{kvp.Key}_{++i}",
                            Value = token
                        });
                    }
                }
            }

        }

        private string ResolveRandom(string key)
        {
            byte[] buffer;

            string result = "";

            string[] seg = key.Split(':');

            int count = 8;

            switch (seg[0])
            {
                case "uid":
                result = Guid.NewGuid().ToString("N");
                break;

                case "hex":
                if (seg.Length < 2 || !int.TryParse(seg[1], out count))
                    count = 8;

                count = Math.Min(count, 256);

                buffer = new byte[count];

                _random.NextBytes(buffer);

                result = BitConverter.ToString(buffer).Replace("-", "");

                break;

                case "b64":
                if (seg.Length < 2 || !int.TryParse(seg[1], out count))
                    count = 16;

                count = Math.Min(count, 256);

                buffer = new byte[count];

                _random.NextBytes(buffer);

                result = Convert.ToBase64String(buffer);

                break;

                case "list":
                if (seg.Length < 3 || !int.TryParse(seg[1], out count))
                    count = 1;

                var options = seg.Last()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                while (count > 0 && options.Count > 0)
                {
                    string val = options[_random.Next(options.Count)];
                    result += val + " ";
                    options.Remove(val);
                    count -= 1;
                }

                result = result.Trim();
                break;

                case "int":
                default:
                int min = 0;
                int max = int.MaxValue;
                if (seg.Length > 1)
                {
                    string[] range = seg[1].Split('-');
                    if (range.Length > 1)
                    {
                        int.TryParse(range[0], out min);
                        int.TryParse(range[1], out max);
                    }
                    else
                    {
                        int.TryParse(range[0], out max);
                    }
                }
                result = _random.Next(min, max).ToString();
                break;

            }

            return result;
        }

        private async Task Deploy(TopoMojo.Api.Data.Gamespace gamespace)
        {
            var tasks = new List<Task<Vm>>();

            var spec = JsonSerializer.Deserialize<ChallengeSpec>(gamespace.Challenge ?? "{}", jsonOptions);

            var isoTargets = (spec.Challenge.Iso.Targets ?? "")
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var templates = Mapper.Map<List<ConvergedTemplate>>(gamespace.Workspace.Templates);

            foreach (var template in templates.ToList())
            {
                // apply template macro substitutions
                foreach (var kvp in spec.Transforms)
                {
                    template.Guestinfo = template.Guestinfo?.Replace($"##{kvp.Key}##", kvp.Value);
                    template.Detail = template.Detail?.Replace($"##{kvp.Key}##", kvp.Value);
                }

                // apply challenge iso
                if (string.IsNullOrEmpty(template.Iso) && isoTargets.Contains(template.Name.ToLower()))
                    template.Iso = $"{spec.Challenge.Iso.File}";

                // expand replicas
                int replicas = template.Replicas < 0 ? gamespace.Players.Count : Math.Min(template.Replicas, _options.ReplicaLimit);

                if (replicas > 1)
                {
                    for (int i = 1; i < replicas; i++)
                    {
                        var tt = template.Clone<ConvergedTemplate>();

                        tt.Name += $"_{i+1}";

                        templates.Add(tt);
                    }

                    template.Name += "_1";
                }
            }

            foreach (var template in templates)
            {
                tasks.Add(
                    _pod.Deploy(
                        template
                        .ToVirtualTemplate(gamespace.Id)
                        .SetHostAffinity(gamespace.Workspace.HostAffinity)
                    )
                );
            }

            await Task.WhenAll(tasks.ToArray());

            if (gamespace.Workspace.HostAffinity)
            {
                var vms = tasks.Select(t => t.Result).ToArray();

                await _pod.SetAffinity(gamespace.Id, vms, true);

                foreach (var vm in vms)
                    vm.State = VmPowerState.Running;
            }

            if (gamespace.StartTime == DateTime.MinValue)
            {
                gamespace.StartTime = DateTime.UtcNow;
                await _store.Update(gamespace);
            }
        }

        private async Task<GameState> LoadState(TopoMojo.Api.Data.Gamespace gamespace, bool preview = false)
        {
            var state = Mapper.Map<GameState>(gamespace);

            state.Markdown = await LoadMarkdown(gamespace.Workspace?.Id)
                ?? $"# {gamespace.Name}";

            if (!preview && !gamespace.HasStarted)
            {
                state.Markdown = state.Markdown.Split("<!-- cut -->").FirstOrDefault();
            }

            if (gamespace.IsActive)
            {
                state.Vms = (await _pod.Find(gamespace.Id))
                    .Select(vm => new VmState
                    {
                        Id = vm.Id,
                        Name = vm.Name.Untagged(),
                        IsolationId = vm.Name.Tag(),
                        IsRunning = (vm.State == VmPowerState.Running),
                        IsVisible = gamespace.IsTemplateVisible(vm.Name)
                    })
                    .Where(s => s.IsVisible)
                    .OrderBy(s => s.Name)
                    .ToArray();
            }

            if (preview || gamespace.HasStarted)
            {
                var spec = JsonSerializer.Deserialize<ChallengeSpec>(gamespace.Challenge, jsonOptions);

                if (spec.Challenge == null)
                    return state;

                // TODO: get active question set

                // map challenge to safe model
                state.Challenge = MapChallenge(spec, gamespace.IsActive, 0);
            }

            return state;
        }

        public async Task Delete(string id)
        {
            var ctx = await LoadContext(id);

            await _pod.DeleteAll(id);

            if (!ctx.Gamespace.AllowReset)
                throw new ActionForbidden();

            await _store.Delete(ctx.Gamespace.Id);
        }

        public async Task<Player[]> Players(string id)
        {
            return Mapper.Map<Player[]>(
                await _store.LoadPlayers(id)
            );
        }

        public async Task<JoinCode> GenerateInvitation(string id)
        {
            Task[] tasks;

            var opts = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 30, 0)
            };

            // remove existing code/key
            string codekey = $"in:{id}";

            string code = await _distCache.GetStringAsync(codekey);

            if (code.NotEmpty())
            {
                tasks = new Task[] {
                    _distCache.RemoveAsync(code),
                    _distCache.RemoveAsync(codekey)
                };

                await Task.WhenAll(tasks);
            }

            // store new code/key
            code = Guid.NewGuid().ToString("n");

            tasks = new Task[] {
                _distCache.SetStringAsync(code, id, opts),
                _distCache.SetStringAsync(codekey, code, opts)
            };

            await Task.WhenAll(tasks);

            return new JoinCode
            {
                Id = id,
                Code = code
            };
        }

        public async Task Enlist(string code, User actor)
        {
            string id = await _distCache.GetStringAsync(code);

            if (id.IsEmpty())
                throw new InvalidInvitation();

            var gamespace = await _store.Load(id);

            if (gamespace.Players.Any(m => m.SubjectId == actor.Id))
                return;

            gamespace.Players.Add(new Data.Player
            {
                SubjectId = actor.Id,
                SubjectName = actor.Name,
            });

            await _store.Update(gamespace);

        }

        public async Task Delist(string id, string subjectId)
        {
            await _store.DeletePlayer(id, subjectId);
        }

        public async Task<ChallengeView> Grade(string id, SectionSubmission submission)
        {
            if (! await _locker.Lock(id))
                throw new ResourceIsLocked();

            var ctx = await LoadContext(id);

            var spec = JsonSerializer.Deserialize<ChallengeSpec>(ctx.Gamespace.Challenge, jsonOptions);

            var section = spec.Challenge.Sections.ElementAtOrDefault(submission.SectionIndex);

            if (section == null)
                _locker.Unlock(id, new InvalidOperationException()).Wait();

            if (!ctx.Gamespace.IsActive)
                _locker.Unlock(id, new GamespaceIsExpired()).Wait();

            if (spec.Submissions.Where(s => s.SectionIndex == submission.SectionIndex).Count() >= spec.MaxAttempts)
                _locker.Unlock(id, new AttemptLimitReached()).Wait();

            submission.Timestamp = DateTime.UtcNow;
            spec.Submissions.Add(submission);

            // grade and save
            int i = 0;
            foreach (var question in section.Questions)
                question.Grade(submission.Questions.ElementAtOrDefault(i++)?.Answer ?? "");

            section.Score = section.Questions
                .Where(q => q.IsCorrect)
                .Select(q => q.Weight - q.Penalty)
                .Sum();

            spec.Score = spec.Challenge.Sections
                .SelectMany(s => s.Questions)
                .Where(q => q.IsCorrect)
                .Select(q => q.Weight - q.Penalty)
                .Sum();

            ctx.Gamespace.Challenge = JsonSerializer.Serialize(spec, jsonOptions);

            // handle completion if max attempts reached or full score
            if (
                spec.Score == 1 ||
                (
                    spec.MaxAttempts > 0 &&
                    spec.Submissions.Count == spec.MaxAttempts
                )
            )
            {
                ctx.Gamespace.EndTime = DateTime.UtcNow;
                await Stop(ctx.Gamespace.Id);
            }

            await _store.Update(ctx.Gamespace);

            // map return model
            var result = MapChallenge(spec, ctx.Gamespace.IsActive);

            // merge submission into return model
            i = 0;
            foreach (var question in result.Questions)
                question.Answer = submission.Questions.ElementAtOrDefault(i++)?.Answer ?? "";

            await _locker.Unlock(id);

            return result;
        }

        private ChallengeView MapChallenge(ChallengeSpec spec, bool isActive, int sectionIndex = 0)
        {
            var section = spec.Challenge.Sections.ElementAtOrDefault(sectionIndex);

            var challenge = new ChallengeView
            {
                IsActive = isActive,
                Text = string.Join("\n\n", spec.Text, spec.Challenge.Text),
                MaxPoints = spec.MaxPoints,
                MaxAttempts = spec.MaxAttempts,
                Attempts = spec.Submissions.Count,
                Score = Math.Round(spec.Score * spec.MaxPoints, 0, MidpointRounding.AwayFromZero),
                SectionIndex = sectionIndex,
                SectionCount = spec.Challenge.Sections.Count,
                SectionScore = Math.Round(section.Score * spec.MaxPoints, 0, MidpointRounding.AwayFromZero),
                SectionText = section.Text,
                Questions = Mapper.Map<QuestionView[]>(section.Questions)
            };

            foreach(var q in challenge.Questions)
            {
                q.Weight = (float) Math.Round(q.Weight * spec.MaxPoints, 0, MidpointRounding.AwayFromZero);
                q.Penalty = (float) Math.Round(q.Penalty * spec.MaxPoints, 0, MidpointRounding.AwayFromZero);
            }

            return challenge;
        }

        private async Task<RegistrationContext> LoadContext(GamespaceRegistration reg)
        {
            return new RegistrationContext
            {
                Request = reg,
                Workspace = await _workspaceStore.Load(reg.ResourceId)
            };
        }

        private async Task<RegistrationContext> LoadContext(string id, string resourceId = null)
        {
            var ctx = new RegistrationContext();

            if (id.NotEmpty())
            {
                ctx.Gamespace = await _store.Load(id);

                ctx.Workspace = ctx.Gamespace?.Workspace;
            }

            if (resourceId.NotEmpty())
                ctx.Workspace = await _workspaceStore.Load(resourceId);

            return ctx;
        }

        private async Task<string> LoadMarkdown(string id)
        {
            string path = System.IO.Path.Combine(
                _options.DocPath,
                id
            ) + ".md";

            return id.NotEmpty() && System.IO.File.Exists(path)
                ? await System.IO.File.ReadAllTextAsync(path)
                : null;
        }

        public async Task<bool> CanManage(string id, string actorId)
        {
            return await _store.CanManage(id, actorId);
        }

        public async Task<bool> CanInteract(string id, string actorId)
        {
            return await _store.CanInteract(id, actorId);
        }

        public async Task<bool> HasValidUserScope(string id, string scope)
        {
            return await _store.HasValidUserScope(id, scope);
        }
    }
}