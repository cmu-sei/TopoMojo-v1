// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models.Extensions;
using TopoMojo.Data;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;
using TopoMojo.Data.Entities.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.Core
{
    public class TopologyManager : EntityManager<Topology>
    {
        public TopologyManager(
            ITopologyRepository repo,
            IGamespaceRepository gameRepo,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            IPodManager podManager
        ) : base (mill, options, profileResolver)
        {
            _repo = repo;
            _gameRepo = gameRepo;
            _pod = podManager;
        }

        private readonly ITopologyRepository _repo;
        private readonly IGamespaceRepository _gameRepo;
        private readonly IPodManager _pod;

        public IQueryable<Topology> GetTopoQuery(Models.Search search)
        {
            if (search.Take == 0) search.Take = 50;

            string[] allowedFilters = new string[] { "private", "public", "detail" };

            if (!Profile.IsAdmin)
            {
                search.Filters = search.Filters.Except(new string[] { "detail" }).ToArray();
            }

            if (!search.Filters.Intersect(allowedFilters).Any())
            {
                search.Filters = new string[] { "private" };
            }

            IQueryable<Topology> q = _repo.List();

            if (search.HasFilter("detail"))
            {
             q = q.Include(t => t.Templates)
                .Include(t => t.Workers)
                .ThenInclude(w => w.Person);
            }

            if (search.Term.HasValue())
            {
                //TODO: Convert to search tags
                q = q.Where(o =>
                    o.Name.IndexOf(search.Term, StringComparison.OrdinalIgnoreCase) >= 0
                    || o.Description.IndexOf(search.Term, StringComparison.OrdinalIgnoreCase) >= 0
                    || o.Author.IndexOf(search.Term, StringComparison.OrdinalIgnoreCase) >= 0
                    || o.GlobalId.IndexOf(search.Term, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }

            if (search.HasFilter("public"))
                q = q.Where(t => t.IsPublished);

            if (search.HasFilter("private"))
                q = q.Where(p => p.Workers.Select(w => w.PersonId).Contains(Profile.Id));

            if (search.Sort == "age")
            {
                q = q.OrderByDescending(o => o.WhenCreated);
            }
            else
            {
                q = q.OrderBy(o => o.Name);
            }
            return q;
        }

        public async Task<Models.SearchResult<Models.TopologySummary>> List(Models.Search search)
        {
            var q = GetTopoQuery(search);

            var result = new Models.SearchResult<Models.TopologySummary>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  Mapper.Map<Models.TopologySummary[]>(q
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArray(), WithActor());
            return result;
        }

        public async Task<Models.SearchResult<Models.Topology>> ListDetail(Models.Search search)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            if (!search.HasFilter("detail"))
            {
                var filters = search.Filters.ToList();
                filters.Add("detail");
                search.Filters = filters.ToArray();
            }

            var q = GetTopoQuery(search);

            Models.SearchResult<Models.Topology> result = new Models.SearchResult<Models.Topology>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  Mapper.Map<Models.Topology[]>(q
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArray(), WithActor());
            return result;
        }

        public async Task<Models.Topology> Load(int id)
        {
            Data.Entities.Topology topo = await _repo.Load(id);
            if (topo == null)
                throw new InvalidOperationException();

            var worker = topo.Workers.Where(w => w.PersonId == Profile.Id).SingleOrDefault();
            if (worker != null)
            {
                worker.LastSeen = DateTime.UtcNow;
                await _repo.Update(topo);
            }

            return Mapper.Map<Models.Topology>(topo, WithActor());
        }

        public async Task<Models.Topology> Create(Models.NewTopology model)
        {
            if (!Profile.IsAdmin)
            {
                int existingWorkspaceCount = await _repo.GetWorkspaceCount(Profile.Id);
                if (existingWorkspaceCount >= Profile.WorkspaceLimit)
                    throw new WorkspaceLimitException();
            }

            Data.Entities.Topology topo = Mapper.Map<Data.Entities.Topology>(model);
            topo.TemplateLimit = _options.WorkspaceTemplateLimit;
            topo.ShareCode = Guid.NewGuid().ToString("N");
            topo.Author = Profile.Name;
            topo.LastLaunch = DateTime.MinValue;
            topo = await _repo.Add(topo);
            topo.Workers.Add(new Worker
            {
                PersonId = Profile.Id,
                Permission = Permission.Manager,
                LastSeen = DateTime.UtcNow
            });
            await _repo.Update(topo);

            return Mapper.Map<Models.Topology>(topo, WithActor());
        }

        public async Task<Models.Topology> Update(Models.ChangedTopology model)
        {
            if (! await _repo.CanEdit(model.Id, Profile))
                throw new InvalidOperationException();

            Data.Entities.Topology entity = await _repo.Load(model.Id);
            if (entity == null)
                throw new InvalidOperationException();

            Mapper.Map<Models.ChangedTopology, Data.Entities.Topology>(model, entity);
            await _repo.Update(entity);
            return Mapper.Map<Models.Topology>(entity, WithActor());
        }

        public async Task<Models.Topology> UpdatePrivilegedChanges(Models.PrivilegedWorkspaceChanges model)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            Data.Entities.Topology entity = await _repo.Load(model.Id);
            if (entity == null)
                throw new InvalidOperationException();

            Mapper.Map<Models.PrivilegedWorkspaceChanges, Data.Entities.Topology>(model, entity);
            await _repo.Update(entity);
            return Mapper.Map<Models.Topology>(entity);
        }

        public async Task<Models.Topology> Delete(int id)
        {
            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            foreach (Vm vm in await _pod.Find(topology.GlobalId))
                await _pod.Delete(vm.Id);

            await _repo.Remove(topology);
            return Mapper.Map<Models.Topology>(topology);
        }

        public async Task<bool> CanEdit(string guid)
        {
            Data.Entities.Topology topology = await _repo.FindByGlobalId(guid);
            if (topology == null)
                return false;

            return await _repo.CanEdit(topology.Id, Profile);
        }

        public async Task<bool> CanEdit(int topoId)
        {
            return await _repo.CanEdit(topoId, Profile);
        }

        public async Task<Models.TopologyState> ChangeState(Models.TopologyStateAction action)
        {
            Models.TopologyState state = null;
            switch (action.Type)
            {
                case Models.TopologyStateActionType.Share:
                state = await Share(action.Id, false);
                break;
                case Models.TopologyStateActionType.Unshare:
                state = await Share(action.Id, true);
                break;
                case Models.TopologyStateActionType.Publish:
                state = await Publish(action.Id, false);
                break;
                case Models.TopologyStateActionType.Unpublish:
                state = await Publish(action.Id, true);
                break;
                case Models.TopologyStateActionType.Lock:
                state = await Lock(action.Id, false);
                break;
                case Models.TopologyStateActionType.Unlock:
                state = await Lock(action.Id, true);
                break;
            }
            return state;
        }
        public async Task<Models.TopologyState> Share(int id, bool revoke)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            // topology.ShareCode = (revoke) ? "" : Guid.NewGuid().ToString("N");
            topology.ShareCode = Guid.NewGuid().ToString("N");
            await _repo.Update(topology);
            return Mapper.Map<Models.TopologyState>(topology);
        }

        public async Task<Models.TopologyState> Publish(int id, bool revoke)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            topology.IsPublished = !revoke;
            if (topology.IsPublished && topology.WhenPublished is null)
            {
                topology.WhenPublished = DateTime.UtcNow;
            }
            await _repo.Update(topology);
            return Mapper.Map<Models.TopologyState>(topology);
        }

        public async Task<Models.TopologyState> Lock(int id, bool revoke)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! Profile.IsAdmin)
                throw new InvalidOperationException();

            topology.IsLocked = !revoke;
            await _repo.Update(topology);
            return Mapper.Map<Models.TopologyState>(topology);
        }

        public async Task<bool> Enlist(string code)
        {
            Topology topology = await _repo.FindByShareCode(code);

            if (topology == null)
                throw new InvalidOperationException();

            if (!topology.Workers.Where(m => m.PersonId == Profile.Id).Any())
            {
                topology.Workers.Add(new Worker
                {
                    PersonId = Profile.Id,
                    Permission = Permission.Editor,
                    LastSeen = DateTime.UtcNow
                });
                await _repo.Update(topology);
            }
            return true;
        }

        public async Task<bool> Delist(int workerId)
        {
            Topology topology = await _repo.FindByWorker(workerId);

            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanManage(topology.Id, Profile))
                throw new InvalidOperationException();

            Worker member = topology.Workers
                .Where(p => p.Id == workerId)
                .SingleOrDefault();

            if (!Profile.IsAdmin //if you aren't admin, you can't remove the last remaining workspace manager
                && member.Permission.CanManage()
                && topology.Workers.Count(w => w.Permission.HasFlag(Permission.Manager)) == 1)
                throw new InvalidOperationException();

            if (member != null)
            {
                topology.Workers.Remove(member);
                await _repo.Update(topology);
            }
            return true;
        }

        public async Task<bool> HasGames(int id)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            return topology.Gamespaces.Any();
        }

        public async Task<GameState[]> GetGames(int id)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            return Mapper.Map<GameState[]>(topology.Gamespaces);
        }

        public async Task<GameState[]> KillGames(int id)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            var result = topology.Gamespaces.ToArray();
            foreach (var gamespace in result)
            {
                List<Task<TopoMojo.Models.Virtual.Vm>> tasks = new List<Task<TopoMojo.Models.Virtual.Vm>>();
                foreach (TopoMojo.Models.Virtual.Vm vm in await _pod.Find(gamespace.GlobalId))
                    tasks.Add(_pod.Delete(vm.Id));
                Task.WaitAll(tasks.ToArray());
                //TODO: _pod.DeleteMatches(player.Gamespace.GlobalId);

                await _gameRepo.Remove(gamespace);
            }

            return Mapper.Map<GameState[]>(result);
        }
    }
}
