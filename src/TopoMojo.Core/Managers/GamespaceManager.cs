// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models.Extensions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public class GamespaceManager : EntityManager<Data.Entities.Gamespace>
    {
        public GamespaceManager(
            IGamespaceRepository repo,
            ITopologyRepository topos,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            IPodManager podManager
        ) : base (mill, options, profileResolver)
        {
            _pod = podManager;
            _repo = repo;
            _topos = topos;
        }

        private readonly IPodManager _pod;
        private readonly IGamespaceRepository _repo;
        private readonly ITopologyRepository _topos;

        public async Task<Gamespace[]> List(string filter)
        {
            if (filter == "all")
            {
                return await ListAll();
            }
            var list = await _repo.ListByProfile(Profile.Id).ToArrayAsync();
            return Mapper.Map<Gamespace[]>(list);
        }

        public async Task<Gamespace[]> ListAll()
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            var list = await _repo.List()
                .Include(g => g.Players)
                .ThenInclude(p => p.Person)
                .ToArrayAsync();
            return Mapper.Map<Gamespace[]>(list);
        }

        public async Task<GameState> Launch(int topoId)
        {
            var gamespaces = await _repo.ListByProfile(Profile.Id).ToArrayAsync();

            var game = gamespaces
                .Where(m => m.TopologyId == topoId)
                .SingleOrDefault();

            if (game == null)
            {
                var topology = await _topos.Load(topoId);
                if (topology == null)
                    throw new InvalidOperationException();

                if (gamespaces.Length >= _options.ConcurrentInstanceMaximum)
                    throw new GamespaceLimitException();

                game = new Data.Entities.Gamespace
                {
                    Name = topology.Name,
                    TopologyId = topoId,
                    ShareCode = Guid.NewGuid().ToString("N")
                };
                game.Players.Add(
                    new Data.Entities.Player
                    {
                        PersonId = Profile.Id,
                        Permission = Data.Entities.Permission.Manager,
                        LastSeen = DateTime.UtcNow
                    }
                );
                await _repo.Add(game);
            }
            return await Deploy(await _repo.Load(game.Id));
        }

        private async Task<GameState> Deploy(Data.Entities.Gamespace gamespace)
        {
            List<Task<TopoMojo.Models.Virtual.Vm>> tasks = new List<Task<TopoMojo.Models.Virtual.Vm>>();
            foreach (var template in gamespace.Topology.Templates)
            {
                TemplateUtility tu = new TemplateUtility(template.Detail ?? template.Parent.Detail);
                tu.Name = template.Name;
                tu.Networks = template.Networks;
                tu.Iso = template.Iso;
                tu.IsolationTag = gamespace.GlobalId;
                tu.Id = template.Id.ToString();
                tasks.Add(_pod.Deploy(tu.AsTemplate()));
            }
            Task.WaitAll(tasks.ToArray());

            return await LoadState(gamespace, gamespace.TopologyId);
        }

        public async Task<GameState> Load(int id)
        {
            var gamespace = await _repo.Load(id);
            return await LoadState(gamespace, gamespace.TopologyId);
        }

        public async Task<GameState> LoadFromTopo(int topoId)
        {
            var gamespace = await _repo.FindByContext(topoId, Profile.Id);
            return await LoadState(gamespace, topoId);
        }

        public async Task<GameState> LoadPreview(int topoId)
        {
            return await LoadState(null, topoId);
        }

        private async Task<GameState> LoadState(Data.Entities.Gamespace gamespace, int topoId)
        {
            GameState state = null;

            if (gamespace == null)
            {
                Data.Entities.Topology topo = await _topos.Load(topoId);
                if (topo == null || !topo.IsPublished)
                    throw new InvalidOperationException();

                state = new GameState();
                state.Name = gamespace?.Name ?? topo.Name;
                state.TopologyDocument = topo.Document;
                state.Vms = topo.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
            }
            else
            {
                var player = gamespace.Players.Where(p => p.PersonId == Profile.Id).Single();
                player.LastSeen = DateTime.UtcNow;
                await _repo.Update(gamespace);

                state = Mapper.Map<GameState>(gamespace);
                state.Vms = gamespace.Topology.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
                state.MergeVms(await _pod.Find(gamespace.GlobalId));
            }

            return state;
        }

        public async Task<GameState> Destroy(int id)
        {
            var gamespace = await _repo.Load(id);
            if (gamespace == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            List<Task<TopoMojo.Models.Virtual.Vm>> tasks = new List<Task<TopoMojo.Models.Virtual.Vm>>();
            foreach (TopoMojo.Models.Virtual.Vm vm in await _pod.Find(gamespace.GlobalId))
                tasks.Add(_pod.Delete(vm.Id));
            Task.WaitAll(tasks.ToArray());
            //TODO: _pod.DeleteMatches(player.Gamespace.GlobalId);

            await _repo.Remove(gamespace);
            return Mapper.Map<GameState>(gamespace);
        }

        public async Task<Player[]> Players(int id)
        {
            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            var players = await _repo.ListPlayers(id).ToArrayAsync();

            return Mapper.Map<Player[]>(players);
        }

        public async Task<bool> Enlist(string code)
        {
            var gamespace = await _repo.FindByShareCode(code);
            if (gamespace == null)
                throw new InvalidOperationException();

            if (!gamespace.Players.Where(m => m.PersonId == Profile.Id).Any())
            {
                gamespace.Players.Add(new Data.Entities.Player
                {
                    PersonId = Profile.Id,
                });
                await _repo.Update(gamespace);
            }
            return true;
        }

        public async Task<bool> Delist(int playerId)
        {
            var gamespace = await _repo.FindByPlayer(playerId);

            if (gamespace == null)
                throw new InvalidOperationException();

            if (! await _repo.CanManage(gamespace.Id, Profile))
                throw new InvalidOperationException();

            var member = gamespace.Players
                .Where(p => p.PersonId == playerId)
                .SingleOrDefault();

            if (member != null)
            {
                gamespace.Players.Remove(member);
                await _repo.Update(gamespace);
            }
            return true;
        }
    }
}
