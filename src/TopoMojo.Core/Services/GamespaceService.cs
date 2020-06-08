// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        private readonly IHypervisorService _pod;
        private readonly IGamespaceStore _gamespaceStore;
        private readonly IWorkspaceStore _workspaceStore;

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

        public async Task<GameState> Launch(int workspaceId)
        {
            var gamespaces = await _gamespaceStore
                .ListByProfile(User.Id)
                .ToArrayAsync();

            var game = gamespaces
                .Where(m => m.WorkspaceId == workspaceId)
                .SingleOrDefault();

            if (game == null)
            {
                var workspace = await _workspaceStore.Load(workspaceId);

                if (workspace == null)
                    throw new InvalidOperationException();

                if (gamespaces.Length >= _options.GamespaceLimit)
                    throw new GamespaceLimitReachedException();

                game = new Data.Gamespace
                {
                    Name = workspace.Name,
                    Workspace = workspace,
                    LastActivity = DateTime.UtcNow,
                    ShareCode = Guid.NewGuid().ToString("N"),
                    Audience = "topomojo"
                };

                game.Players.Add(
                    new Data.Player
                    {
                        PersonId = User.Id,
                        Permission = Data.Permission.Manager
                        // LastSeen = DateTime.UtcNow
                    }
                );

                await _gamespaceStore.Add(game);
            }

            return await Deploy(await _gamespaceStore.Load(game.Id));
        }

        private async Task<GameState> Deploy(Data.Gamespace gamespace)
        {
            var tasks = new List<Task<Vm>>();

            var templates = Mapper.Map<List<ConvergedTemplate>>(gamespace.Workspace.Templates);

            foreach (var template in templates)
            {
                tasks.Add(
                    _pod.Deploy(
                        template.ToVirtualTemplate(gamespace.GlobalId)
                    )
                );
            }

            Task.WaitAll(tasks.ToArray());

            return await LoadState(gamespace, gamespace.WorkspaceId);
        }

        public async Task<GameState> Load(int id)
        {
            var gamespace = await _gamespaceStore.Load(id);

            return await LoadState(gamespace, gamespace.WorkspaceId);
        }

        public async Task<GameState> LoadFromWorkspace(int workspaceId)
        {
            var gamespace = User.Id > 0
                ? await _gamespaceStore.FindByContext(workspaceId, User.Id)
                : null;

            return await LoadState(gamespace, workspaceId);
        }

        private async Task<GameState> LoadState(Data.Gamespace gamespace, int workspaceId)
        {
            GameState state = null;

            if (gamespace == null)
            {
                var workspace = await _workspaceStore.Load(workspaceId);

                if (workspace == null || !workspace.IsPublished)
                    throw new InvalidOperationException();

                state = new GameState();

                state.Name = gamespace?.Name ?? workspace.Name;

                state.WorkspaceDocument = workspace.Document;

                state.Vms = workspace.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
            }
            else
            {
                state = Mapper.Map<GameState>(gamespace);

                state.Vms = gamespace.Workspace.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();

                state.MergeVms(await _pod.Find(gamespace.GlobalId));
            }

            return state;
        }

        public async Task<GameState> Destroy(int id)
        {
            var gamespace = await _gamespaceStore.Load(id);

            if (gamespace == null || !gamespace.CanEdit(User))
                throw new InvalidOperationException();

            await _pod.DeleteAll(gamespace.GlobalId);

            await _gamespaceStore.Delete(id);

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
