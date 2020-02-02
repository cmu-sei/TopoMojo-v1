// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Workspace;

namespace TopoMojo.Core
{
    public class WorkspaceService : EntityService<Data.Topology>
    {
        public WorkspaceService(
            IWorkspaceStore workspaceStore,
            IGamespaceStore gamespaceStore,
            IHypervisorService podService,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        ) : base (mill, options, profileResolver)
        {
            _workspaceStore = workspaceStore;
            _gamespaceStore = gamespaceStore;
            _pod = podService;
        }

        private readonly IWorkspaceStore _workspaceStore;
        private readonly IGamespaceStore _gamespaceStore;
        private readonly IHypervisorService _pod;

        public IQueryable<Data.Topology> GetTopoQuery(Search search)
        {
            if (search.Take == 0) search.Take = 50;

            string[] allowedFilters = new string[] { "private", "public", "detail" };

            if (!User.IsAdmin)
            {
                search.Filters = search.Filters.Except(new string[] { "detail" }).ToArray();
            }

            if (!search.Filters.Intersect(allowedFilters).Any())
            {
                search.Filters = new string[] { "private" };
            }

            var q = _workspaceStore.List();

            if (search.HasFilter("detail"))
            {
                q = q.Include(t => t.Templates)
                    .Include(t => t.Workers)
                    .ThenInclude(w => w.Person);
            }

            if (search.HasFilter("public"))
                q = q.Where(t => t.IsPublished);

            if (search.HasFilter("private"))
                q = q.Where(p => p.Workers.Select(w => w.PersonId).Contains(User.Id));

            if (search.Term.HasValue())
            {
                string term = search.Term.ToLower();
                q = q.Where(o =>
                    o.GlobalId.StartsWith(term)
                    || o.Name.ToLower().Contains(term)
                    || o.Description.ToLower().Contains(term)
                    || o.Author.ToLower().Contains(term)
                );
                // // TODO: Convert to search tags
            }

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

        public async Task<SearchResult<WorkspaceSummary>> List(Search search)
        {
            var q = GetTopoQuery(search);

            var result = new SearchResult<WorkspaceSummary>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  Mapper.Map<WorkspaceSummary[]>(q
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArray(), WithActor());
            return result;
        }

        public async Task<SearchResult<Workspace>> ListDetail(Search search)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            if (!search.HasFilter("detail"))
            {
                var filters = search.Filters.ToList();
                filters.Add("detail");
                search.Filters = filters.ToArray();
            }

            var q = GetTopoQuery(search);

            SearchResult<Workspace> result = new SearchResult<Workspace>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  Mapper.Map<Workspace[]>(q
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArray(), WithActor());
            return result;
        }

        public async Task<Workspace> Load(int id)
        {
            Data.Topology topo = await _workspaceStore.Load(id);
            if (topo == null)
                throw new InvalidOperationException();

            var worker = topo.Workers.Where(w => w.PersonId == User.Id).SingleOrDefault();
            if (worker != null)
            {
                worker.LastSeen = DateTime.UtcNow;
                await _workspaceStore.Update(topo);
            }

            return Mapper.Map<Workspace>(topo, WithActor());
        }

        public async Task<Workspace> Create(NewWorkspace model)
        {
            if (!User.IsAdmin)
            {
                int existingWorkspaceCount = await _workspaceStore.GetWorkspaceCount(User.Id);
                if (existingWorkspaceCount >= User.WorkspaceLimit)
                    throw new WorkspaceLimitException();
            }

            Data.Topology topo = Mapper.Map<Data.Topology>(model);
            topo.TemplateLimit = _options.WorkspaceTemplateLimit;
            topo.ShareCode = Guid.NewGuid().ToString("N");
            topo.Author = User.Name;
            topo.LastLaunch = DateTime.MinValue;
            topo = await _workspaceStore.Add(topo);
            topo.Workers.Add(new Data.Worker
            {
                PersonId = User.Id,
                Permission = Data.Permission.Manager,
                LastSeen = DateTime.UtcNow
            });
            await _workspaceStore.Update(topo);

            return Mapper.Map<Workspace>(topo, WithActor());
        }

        public async Task<Workspace> Update(ChangedWorkspace model)
        {
            if (! await _workspaceStore.CanEdit(model.Id, User))
                throw new InvalidOperationException();

            Data.Topology entity = await _workspaceStore.Load(model.Id);
            if (entity == null)
                throw new InvalidOperationException();

            Mapper.Map<ChangedWorkspace, Data.Topology>(model, entity);
            await _workspaceStore.Update(entity);
            return Mapper.Map<Workspace>(entity, WithActor());
        }

        public async Task<Workspace> UpdatePrivilegedChanges(PrivilegedWorkspaceChanges model)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            Data.Topology entity = await _workspaceStore.Load(model.Id);
            if (entity == null)
                throw new InvalidOperationException();

            Mapper.Map<PrivilegedWorkspaceChanges, Data.Topology>(model, entity);
            await _workspaceStore.Update(entity);
            return Mapper.Map<Workspace>(entity);
        }

        public async Task<Workspace> Delete(int id)
        {
            if (! await _workspaceStore.CanEdit(id, User))
                throw new InvalidOperationException();

            Data.Topology topology = await _workspaceStore.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            foreach (Vm vm in await _pod.Find(topology.GlobalId))
                await _pod.Delete(vm.Id);

            await _workspaceStore.Remove(topology);
            return Mapper.Map<Workspace>(topology);
        }

        public async Task<bool> CanEdit(string guid)
        {
            Data.Topology topology = await _workspaceStore.FindByGlobalId(guid);
            if (topology == null)
                return false;

            return await _workspaceStore.CanEdit(topology.Id, User);
        }

        public async Task<bool> CanEdit(int topoId)
        {
            return await _workspaceStore.CanEdit(topoId, User);
        }

        public async Task<WorkspaceState> ChangeState(WorkspaceStateAction action)
        {
            WorkspaceState state = null;
            switch (action.Type)
            {
                case WorkspaceStateActionType.Share:
                state = await Share(action.Id, false);
                break;
                case WorkspaceStateActionType.Unshare:
                state = await Share(action.Id, true);
                break;
                case WorkspaceStateActionType.Publish:
                state = await Publish(action.Id, false);
                break;
                case WorkspaceStateActionType.Unpublish:
                state = await Publish(action.Id, true);
                break;
                case WorkspaceStateActionType.Lock:
                state = await Lock(action.Id, false);
                break;
                case WorkspaceStateActionType.Unlock:
                state = await Lock(action.Id, true);
                break;
            }
            return state;
        }
        public async Task<WorkspaceState> Share(int id, bool revoke)
        {
            Data.Topology topology = await _workspaceStore.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _workspaceStore.CanEdit(id, User))
                throw new InvalidOperationException();

            // topology.ShareCode = (revoke) ? "" : Guid.NewGuid().ToString("N");
            topology.ShareCode = Guid.NewGuid().ToString("N");
            await _workspaceStore.Update(topology);
            return Mapper.Map<WorkspaceState>(topology);
        }

        public async Task<WorkspaceState> Publish(int id, bool revoke)
        {
            Data.Topology topology = await _workspaceStore.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _workspaceStore.CanEdit(id, User))
                throw new InvalidOperationException();

            topology.IsPublished = !revoke;
            if (topology.IsPublished && topology.WhenPublished is null)
            {
                topology.WhenPublished = DateTime.UtcNow;
            }
            await _workspaceStore.Update(topology);
            return Mapper.Map<WorkspaceState>(topology);
        }

        public async Task<WorkspaceState> Lock(int id, bool revoke)
        {
            Data.Topology topology = await _workspaceStore.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! User.IsAdmin)
                throw new InvalidOperationException();

            topology.IsLocked = !revoke;
            await _workspaceStore.Update(topology);
            return Mapper.Map<WorkspaceState>(topology);
        }

        public async Task<bool> Enlist(string code)
        {
            var topology = await _workspaceStore.FindByShareCode(code);

            if (topology == null)
                throw new InvalidOperationException();

            if (!topology.Workers.Where(m => m.PersonId == User.Id).Any())
            {
                topology.Workers.Add(new Data.Worker
                {
                    PersonId = User.Id,
                    Permission = Data.Permission.Editor,
                    LastSeen = DateTime.UtcNow
                });
                await _workspaceStore.Update(topology);
            }
            return true;
        }

        public async Task<bool> Delist(int workerId)
        {
            var topology = await _workspaceStore.FindByWorker(workerId);

            if (topology == null)
                throw new InvalidOperationException();

            if (! await _workspaceStore.CanManage(topology.Id, User))
                throw new InvalidOperationException();

            var member = topology.Workers
                .Where(p => p.Id == workerId)
                .SingleOrDefault();

            if (!User.IsAdmin //if you aren't admin, you can't remove the last remaining workspace manager
                && member.Permission.CanManage()
                && topology.Workers.Count(w => w.Permission.HasFlag(Data.Permission.Manager)) == 1)
                throw new InvalidOperationException();

            if (member != null)
            {
                topology.Workers.Remove(member);
                await _workspaceStore.Update(topology);
            }
            return true;
        }

        public async Task<bool> HasGames(int id)
        {
            Data.Topology topology = await _workspaceStore.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _workspaceStore.CanEdit(id, User))
                throw new InvalidOperationException();

            return topology.Gamespaces.Any();
        }

        public async Task<GameState[]> GetGames(int id)
        {
            Data.Topology topology = await _workspaceStore.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _workspaceStore.CanEdit(id, User))
                throw new InvalidOperationException();

            return Mapper.Map<GameState[]>(topology.Gamespaces);
        }

        public async Task<GameState[]> KillGames(int id)
        {
            Data.Topology topology = await _workspaceStore.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _workspaceStore.CanEdit(id, User))
                throw new InvalidOperationException();

            var result = topology.Gamespaces.ToArray();

            foreach (var gamespace in result)
            {
                await _pod.DeleteAll(gamespace.GlobalId);
                await _gamespaceStore.Remove(gamespace);
            }

            return Mapper.Map<GameState[]>(result);
        }
    }
}
