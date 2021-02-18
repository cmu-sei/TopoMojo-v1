// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Extensions;
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
            IIdentityResolver identityResolver
        ) : base (logger, mapper, options, identityResolver)
        {
            _pod = podService;
            _gamespaceStore = gamespaceStore;
            _workspaceStore = workspaceStore;
            _random = new Random();
        }

        private readonly IHypervisorService _pod;
        private readonly IGamespaceStore _gamespaceStore;
        private readonly IWorkspaceStore _workspaceStore;
        private readonly Random _random;
        public async Task<Gamespace[]> List(string filter, CancellationToken ct = default(CancellationToken))
        {
            if (filter == "all")
            {
                return await ListAll(ct);
            }

            var list = await _gamespaceStore.ListByProfile(User.Id).ToArrayAsync(ct);

            return Mapper.Map<Gamespace[]>(list);
        }

        public async Task<Gamespace[]> ListAll(CancellationToken ct = default(CancellationToken))
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            var list = await _gamespaceStore.List()
                .Include(g => g.Players)
                .ThenInclude(p => p.Person)
                .ToArrayAsync(ct);

            return Mapper.Map<Gamespace[]>(list);
        }

        public async Task<GameState> Launch(Registration registration)
        {
            return await Launch(
                await _workspaceStore.Load(registration.ResourceId),
                registration.GamespaceId
            );
        }

        public async Task<GameState> Launch(int workspaceId)
        {
            return await Launch(
                await _workspaceStore.Load(workspaceId)
            );
        }

        private async Task<GameState> Launch(Data.Workspace workspace, string isolationId = null)
        {
            var gamespaces = await _gamespaceStore
                .ListByProfile(User.Id)
                .ToArrayAsync();

            var game = gamespaces
                .Where(m => m.WorkspaceId == workspace.Id)
                .SingleOrDefault();

            if (game == null)
            {
                if (gamespaces.Length >= _options.GamespaceLimit)
                    throw new GamespaceLimitReachedException();

                game = await Create(workspace, Client.Id, isolationId);
            }

            await Deploy(game);

            var state = await LoadState(game);

            return state;
        }

        private async Task<Data.Gamespace> Create(
            Data.Workspace workspace,
            string client,
            string isolationId
        )
        {

            var gamespace = new Data.Gamespace
            {
                GlobalId = isolationId,
                Name = workspace.Name,
                Workspace = workspace,
                ShareCode = Guid.NewGuid().ToString("n"),
                Audience = client
            };

            gamespace.Players.Add(
                new Data.Player
                {
                    PersonId = User.Id,
                    Permission = Data.Permission.Manager
                    // LastSeen = DateTime.UtcNow
                }
            );

            // glean randomize targets
            var regex = new Regex("##[^#]*##");

            var map = new Dictionary<string, string>();

            foreach (var t in gamespace.Workspace.Templates)
            {
                foreach (Match match in regex.Matches(t.Guestinfo ?? ""))
                    map.TryAdd(match.Value, "");

                foreach (Match match in regex.Matches(t.Detail ?? t.Parent?.Detail ?? ""))
                    map.TryAdd(match.Value, "");
            }

            // clone challenge
            var spec = new ChallengeSpec();
            if (!string.IsNullOrEmpty(workspace.Challenge))
            {
                spec = JsonSerializer.Deserialize<ChallengeSpec>(workspace.Challenge ?? "{}", jsonOptions);

                foreach (var question in spec.Questions)
                    foreach (Match match in regex.Matches(question.Answer))
                    {
                        string key = match.Value;
                        string val = "";

                        if (key.Contains(":list##"))
                        {
                            string[] list = question.Answer
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Skip(1)
                                .ToArray();

                            if (list.Length > 0)
                            {
                                val = list[_random.Next(list.Length)];

                                question.Answer = key;

                                if (map.ContainsKey(key))
                                    map[key] = val;
                            }
                        }

                        map.TryAdd(key, val);
                    }
            }

            // resolve macros
            foreach (string key in map.Keys.ToArray())
                if (string.IsNullOrEmpty(map[key]))
                    map[key] = ResolveRandom(key);

            // apply macros to spec answers
            foreach (var q in spec.Questions)
                foreach (string key in map.Keys)
                    q.Answer = q.Answer.Replace(key, map[key]);

            spec.Randoms = map;

            gamespace.Challenge = JsonSerializer.Serialize(spec, jsonOptions);

            await _gamespaceStore.Add(gamespace);

            return gamespace;
        }

        private string ResolveRandom(string key)
        {
            string result = "";

            string[] seg = key.Replace("#", "").Split(':');

            int count = 8;

            switch (seg[1])
            {
                case "uid":
                result = Guid.NewGuid().ToString("N");
                break;

                case "hex":
                if (seg.Length < 3 || !int.TryParse(seg[2], out count))
                    count = 8;
                count = Math.Min(count, 64);

                while (result.Length < count)
                    result += _random.Next().ToString("x8");

                break;

                case "b64":
                if (seg.Length < 3 || !int.TryParse(seg[2], out count))
                    count = 16;
                count = Math.Min(count, 64);
                byte[] buffer = new byte[count];
                _random.NextBytes(buffer);
                result = Convert.ToBase64String(buffer);
                break;

                case "int":
                default:
                int min = 0;
                int max = int.MaxValue;
                if (seg.Length > 2)
                {
                    string[] range = seg[2].Split('-');
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

            // ##target:type:range##
            // ##jam:hex:8##
            // ##jam:b64:24##
            // ##jam:uid:0##
            // ##jam:int:99-9999##
            // ##jam:list## one two three
            // ##jam:net:x.x.x.0##
            // ##jam:ip4:x.x.x.x##
        }

        private async Task Deploy(Data.Gamespace gamespace)
        {
            var tasks = new List<Task<Vm>>();

            var spec = JsonSerializer.Deserialize<ChallengeSpec>(gamespace.Challenge ?? "{}", jsonOptions);

            var templates = Mapper.Map<List<ConvergedTemplate>>(gamespace.Workspace.Templates);

            foreach (var template in templates)
            {
                // apply template macro substitutions
                foreach (string key in spec.Randoms.Keys)
                {
                    template.Guestinfo = template.Guestinfo?.Replace(key, spec.Randoms[key]);
                    template.Detail = template.Detail?.Replace(key, spec.Randoms[key]);
                }

                // TODO: add template replicas

                tasks.Add(
                    _pod.Deploy(
                        template.ToVirtualTemplate(gamespace.GlobalId)
                    )
                );
            }

            await Task.WhenAll(tasks.ToArray());
        }

        public async Task<GameState> Load(int id)
        {
            var gamespace = await _gamespaceStore.Load(id);

            return await LoadState(gamespace);
        }

        public async Task<GameState> LoadFromWorkspace(int workspaceId)
        {
            var gamespace = User.Id > 0
                ? await _gamespaceStore.FindByContext(workspaceId, User.Id)
                : null;

            if (gamespace is Data.Gamespace)
                return await LoadState(gamespace);

            var workspace = await _workspaceStore.Load(workspaceId);

            return LoadState(workspace).Result;
        }

        private async Task<GameState> LoadState(Data.Gamespace gamespace)
        {
            var state = Mapper.Map<GameState>(gamespace);

            state.Vms = gamespace.Workspace.Templates
                .Where(t => !t.IsHidden)
                .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                .ToArray();

            state.MergeVms(await _pod.Find(gamespace.GlobalId));

            return state;
        }

        private Task<GameState> LoadState(Data.Workspace workspace)
        {
            if (workspace == null || !workspace.IsPublished)
                throw new InvalidOperationException();

            var state = new GameState();

            state.Name = workspace.Name;

            state.WorkspaceDocument = workspace.Document;

            state.Vms = workspace.Templates
                .Where(t => !t.IsHidden)
                .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                .ToArray();

            return Task.FromResult(state);
        }

        public async Task<GameState> Destroy(string id)
        {
            var gamespace = await _gamespaceStore.List()
                .Include(g => g.Players)
                .SingleOrDefaultAsync(g => g.GlobalId == id);

            return await Destroy(gamespace);
        }

        public async Task<GameState> Destroy(int id)
        {
            var gamespace = await _gamespaceStore.Load(id);
            return await Destroy(gamespace);
        }

        private async Task<GameState> Destroy(Data.Gamespace gamespace)
        {
            if (gamespace == null || !gamespace.CanEdit(User))
                return null;

            await _pod.DeleteAll(gamespace.GlobalId);

            await _gamespaceStore.Delete(gamespace.Id);

            return Mapper.Map<GameState>(gamespace);
        }

        public async Task<Player[]> Players(int id)
        {
            var gamespace = await _gamespaceStore.Load(id);

            if (gamespace == null || !gamespace.CanEdit(User))
                throw new InvalidOperationException();

            return Mapper.Map<Player[]>(gamespace.Players);
        }

        public async Task Enlist(string code)
        {
            var gamespace = await _gamespaceStore.FindByShareCode(code);

            if (gamespace == null)
                throw new InvalidOperationException();

            if (!gamespace.Players.Where(m => m.PersonId == User.Id).Any())
            {
                gamespace.Players.Add(new Data.Player
                {
                    PersonId = User.Id,
                });

                await _gamespaceStore.Update(gamespace);
            }

        }

        public async Task Delist(int playerId)
        {
            var gamespace = await _gamespaceStore.FindByPlayer(playerId);

            if (gamespace == null || !gamespace.CanManage(User))
                throw new InvalidOperationException();

            var member = gamespace.Players
                .Where(p => p.PersonId == playerId)
                .SingleOrDefault();

            if (member != null)
            {
                gamespace.Players.Remove(member);

                await _gamespaceStore.Update(gamespace);
            }

        }
    }
}
