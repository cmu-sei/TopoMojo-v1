// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
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
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class GamespaceService : _Service
    {
        public GamespaceService(
            IGamespaceStore gamespaceStore,
            IWorkspaceStore workspaceStore,
            IHypervisorService podService,
            ILogger<GamespaceService> logger,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver,
            ILockService lockService

        ) : base (logger, mapper, options, identityResolver)
        {
            _pod = podService;
            _gamespaceStore = gamespaceStore;
            _workspaceStore = workspaceStore;
            _locker = lockService;
            _random = new Random();
        }

        private readonly IHypervisorService _pod;
        private readonly IGamespaceStore _gamespaceStore;
        private readonly IWorkspaceStore _workspaceStore;
        private readonly ILockService _locker;
        private readonly Random _random;

        public async Task<Gamespace[]> List(string filter, CancellationToken ct = default(CancellationToken))
        {
            if (filter == "all")
            {
                return await ListAll(ct);
            }

            var list = await _gamespaceStore.ListByProfile(User.GlobalId).ToArrayAsync(ct);

            return Mapper.Map<Gamespace[]>(list);
        }

        public async Task<Gamespace[]> ListAll(CancellationToken ct = default(CancellationToken))
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            var list = await _gamespaceStore.List()
                .Include(g => g.Players)
                .ToArrayAsync(ct);

            return Mapper.Map<Gamespace[]>(list);
        }

        public async Task<GameState> Preview(string resourceId)
        {
            var ctx = LoadContext();

            ctx.Workspace = await _workspaceStore.Retrieve(resourceId);

            if (!ctx.WorkspaceExists)
                throw new ResourceNotFound();

            if (!ctx.IsValidAudience)
                throw new InvalidClientAudience();

            if (ctx.UserExists)
            {
                ctx.Gamespace = await _gamespaceStore
                    .FindByContext(ctx.Workspace.Id, User.GlobalId);
            }

            return ctx.GamespaceExists
                ? await LoadState(ctx.Gamespace)
                : new GameState
                    {
                        Name = ctx.Workspace.Name,

                        Markdown = (await LoadMarkdown(ctx.Workspace.GlobalId)).Split("<!-- cut -->").First()
                            ?? $"# {ctx.Workspace.Name}"
                    };
        }

        public async Task<GameState> Register(RegistrationRequest request)
        {
            var gamespace = await _Register(request);

            if (request.StartGamespace)
                await Deploy(gamespace);

            return await LoadState(gamespace, request.AllowPreview);
        }

        private async Task<Data.Gamespace> _Register(RegistrationRequest request)
        {
            if (User.Id > 0)
            {
                request.SubjectId = User.GlobalId;
                request.SubjectName = User.Name;
            }

            var gamespace = await _gamespaceStore.FindByContext(request.SubjectId, request.ResourceId);

            if (gamespace is Data.Gamespace)
                return gamespace;

            string lockKey = $"{request.SubjectId}{request.ResourceId}";

            var ctx = LoadContext(request);

            ctx.Workspace = await _workspaceStore.Retrieve(request.ResourceId);

            if (!ctx.WorkspaceExists)
                throw new ResourceNotFound();

            if (!ctx.IsValidAudience)
                throw new InvalidClientAudience();

            if (Client.GamespaceLimit > 0 && await _gamespaceStore.List().CountAsync(g => g.ClientId == Client.Id) >= Client.GamespaceLimit)
                throw new ClientGamespaceLimitReached();

            if (Client.PlayerGamespaceLimit > 0 && await _gamespaceStore.ListByProfile(ctx.Request.SubjectId).CountAsync(g => g.ClientId == Client.Id) >= Client.PlayerGamespaceLimit)
                throw new PlayerGamespaceLimitReached();

            if (! await _locker.Lock(lockKey))
                throw new ResourceIsLocked();

            try
            {
                await Create(ctx);
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

            if (!ctx.Gamespace.IsExpired())
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

            if (ctx.Gamespace.IsActive())
            {
                ctx.Gamespace.StopTime = DateTime.UtcNow;

                await _gamespaceStore.Update(ctx.Gamespace);
            }

            await _pod.DeleteAll(id);

            return await LoadState(ctx.Gamespace);
        }

        private async Task Create(RegistrationContext ctx)
        {

            var ts = DateTime.UtcNow;

            var gamespace = new Data.Gamespace
            {
                Name = ctx.Workspace.Name,
                Workspace = ctx.Workspace,
                ClientId = ctx.Client.Id,
                AllowReset = ctx.Request.AllowReset,
                WhenCreated = ts,
                ExpirationTime = ctx.Request.ResolveExpiration(ts, ctx.Client.MaxMinutes)
            };

            gamespace.Players.Add(
                new Data.Player
                {
                    WorkspaceId = ctx.Workspace.GlobalId,
                    SubjectId = ctx.Request.SubjectId,
                    SubjectName = ctx.Request.SubjectName,
                    Permission = Permission.Manager
                }
            );

            // clone challenge
            var spec = JsonSerializer.Deserialize<Models.v2.ChallengeSpec>(ctx.Workspace.Challenge ?? "{}", jsonOptions);

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

            await _gamespaceStore.Create(gamespace);

            ctx.Gamespace = gamespace;
        }

        private void ResolveTransforms(Models.v2.ChallengeSpec spec)
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
                        spec.Transforms.Add(new Models.v2.StringKeyValue
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
                count = Math.Min(count, 64);

                while (result.Length < count)
                    result += _random.Next().ToString("x8");

                break;

                case "b64":
                if (seg.Length < 2 || !int.TryParse(seg[1], out count))
                    count = 16;
                count = Math.Min(count, 64);
                byte[] buffer = new byte[count];
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

        private async Task Deploy(Data.Gamespace gamespace)
        {

            var tasks = new List<Task<Vm>>();

            var spec = JsonSerializer.Deserialize<Models.v2.ChallengeSpec>(gamespace.Challenge ?? "{}", jsonOptions);

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
                        template.ToVirtualTemplate(gamespace.GlobalId)
                    )
                );
            }

            await Task.WhenAll(tasks.ToArray());

            if (gamespace.StartTime == DateTime.MinValue)
            {
                gamespace.StartTime = DateTime.UtcNow;
                await _gamespaceStore.Update(gamespace);
            }
        }

        private async Task<GameState> LoadState(Data.Gamespace gamespace, bool preview = false)
        {
            var state = Mapper.Map<GameState>(gamespace);

            state.Markdown = await LoadMarkdown(gamespace.Workspace.GlobalId)
                ?? $"# {gamespace.Name}";

            if (!preview && !gamespace.HasStarted())
            {
                state.Markdown = state.Markdown.Split("<!-- cut -->").FirstOrDefault();
            }

            if (gamespace.IsActive())
            {
                state.Vms = (await _pod.Find(gamespace.GlobalId))
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

            if (preview || gamespace.HasStarted())
            {
                var spec = JsonSerializer.Deserialize<Models.v2.ChallengeSpec>(gamespace.Challenge, jsonOptions);

                if (spec.Challenge == null)
                    return state;

                // TODO: get active question set

                // map challenge to safe model
                state.Challenge = MapChallenge(spec, gamespace.IsActive(), 0);
            }

            return state;
        }

        // private async Task<GameState> LoadState(Data.Workspace workspace)
        // {
        //     var state = new GameState
        //     {
        //         Name = workspace.Name,

        //         Markdown = (await LoadMarkdown(ctx)).Split("<!-- cut -->").First()
        //     };

        //     return state;
        // }

        public async Task Delete(string id)
        {
            var ctx = await LoadContext(id);

            if (!ctx.IsMember || !ctx.Gamespace.AllowReset)
                throw new ActionForbidden();

            await _pod.DeleteAll(id);

            await _gamespaceStore.Delete(ctx.Gamespace.Id);
        }

        public async Task<Player[]> Players(int id)
        {
            var gamespace = await _gamespaceStore.Retrieve(id);

            if (gamespace == null || !gamespace.CanEdit(User))
                throw new InvalidOperationException();

            return Mapper.Map<Player[]>(gamespace.Players);
        }

        public async Task Enlist(string code)
        {
            var gamespace = await _gamespaceStore.FindByShareCode(code);

            if (gamespace == null)
                throw new InvalidOperationException();

            if (!gamespace.Players.Where(m => m.SubjectId == User.GlobalId).Any())
            {
                gamespace.Players.Add(new Data.Player
                {
                    SubjectId = User.GlobalId,
                    SubjectName = User.Name,
                    WorkspaceId = gamespace.Workspace.GlobalId
                });

                await _gamespaceStore.Update(gamespace);
            }

        }

        public async Task Delist(int playerId)
        {
            var gamespace = await _gamespaceStore.FindByPlayer(playerId);

            if (!gamespace?.CanManage(User) ?? false)
                throw new ActionForbidden();

            var member = gamespace.Players
                .Where(p => p.Id == playerId)
                .SingleOrDefault();

            if (member != null)
            {
                gamespace.Players.Remove(member);

                await _gamespaceStore.Update(gamespace);
            }
        }

        public async Task<Models.v2.ChallengeView> Grade(string id, Models.v2.SectionSubmission submission)
        {
            if (! await _locker.Lock(id))
                throw new ResourceIsLocked();

            var ctx = await LoadContext(id);

            var spec = JsonSerializer.Deserialize<Models.v2.ChallengeSpec>(ctx.Gamespace.Challenge, jsonOptions);

            var section = spec.Challenge.Sections.ElementAtOrDefault(submission.SectionIndex);

            if (section == null)
                _locker.Unlock(id, new InvalidOperationException()).Wait();

            if (ctx.Gamespace.IsExpired())
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
                ctx.Gamespace.StopTime = DateTime.UtcNow;
                await Stop(ctx.Gamespace.GlobalId);
            }

            await _gamespaceStore.Update(ctx.Gamespace);

            // map return model
            var result = MapChallenge(spec, ctx.Gamespace.IsActive());

            // merge submission into return model
            i = 0;
            foreach (var question in result.Questions)
                question.Answer = submission.Questions.ElementAtOrDefault(i++)?.Answer ?? "";

            await _locker.Unlock(id);

            return result;
        }

        private Models.v2.ChallengeView MapChallenge(Models.v2.ChallengeSpec spec, bool isActive, int sectionIndex = 0)
        {
            var section = spec.Challenge.Sections.ElementAtOrDefault(sectionIndex);

            var challenge = new Models.v2.ChallengeView
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
                Questions = Mapper.Map<Models.v2.QuestionView[]>(section.Questions)
            };

            foreach(var q in challenge.Questions)
            {
                q.Weight = (float) Math.Round(q.Weight * spec.MaxPoints, 0, MidpointRounding.AwayFromZero);
                q.Penalty = (float) Math.Round(q.Penalty * spec.MaxPoints, 0, MidpointRounding.AwayFromZero);
            }

            return challenge;
        }

        private RegistrationContext LoadContext(RegistrationRequest reg)
        {
            var ctx = LoadContext();
            ctx.Request = reg;
            return ctx;
        }

        private async Task<RegistrationContext> LoadContext(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ResourceNotFound();

            var ctx = LoadContext();

            ctx.Gamespace = await _gamespaceStore.Retrieve(id);

            ctx.Workspace = ctx.Gamespace?.Workspace;

            if (ctx.Gamespace == null)
                throw new ResourceNotFound();

            if (!ctx.IsMember)
                throw new ActionForbidden();

            return ctx;
        }

        private RegistrationContext LoadContext()
        {
            return new RegistrationContext
            {
                User = User,
                Client = Client,
            };
        }

        private async Task<string> LoadMarkdown(string id)
        {
            string path = System.IO.Path.Combine(
                _options.DocPath,
                id
            ) + ".md";

            return System.IO.File.Exists(path)
                ? await System.IO.File.ReadAllTextAsync(path)
                : null;
        }
    }

    public class RegistrationContext
    {
        public RegistrationRequest Request { get; set; }
        public Data.Gamespace Gamespace { get; set; }
        public Data.Workspace Workspace { get; set; }
        public Client Client { get; set; }
        public User User { get; set; }

        public bool UserExists { get { return User is User && User.Id > 0; } }

        public bool WorkspaceExists { get { return Workspace is Data.Workspace; } }

        public bool GamespaceExists { get { return Gamespace is Data.Gamespace; } }

        public bool IsValidAudience
        {
            get
            {
                return UserExists
                    ? Workspace.CanEdit(User)
                    : (Workspace ?? Gamespace.Workspace).HasScope(Client.Scope);
            }
        }

        public bool IsMember
        {
            get
            {
                return UserExists
                    ? Gamespace.CanManage(User)
                    : Gamespace.ClientId == Client.Id;
            }
        }
    }
}
