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

        public async Task<Models.SearchResult<Models.TopologySummary>> List(Models.Search search)
        {
            string[] allowedFilters = new string[] { "mine", "published" };
            if (!Profile.IsAdmin && !search.Filters.Intersect(allowedFilters).Any())
                search.Filters = new string[] { "mine" };

            IQueryable<Topology> q = _repo.List();
            if (search.Term.HasValue())
            {
                q = q.Where(o =>
                    o.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || o.Description.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || o.GlobalId.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0
                );
            }

            if (search.HasFilter("published"))
                q = q.Where(t => t.IsPublished);

            if (search.HasFilter("mine"))
                q = q.Where(p => p.Workers.Select(w => w.PersonId).Contains(Profile.Id));

            return await ProcessQuery(search, q);
        }

        // public async Task<SearchResult<Models.Topology>> ListMine(Search search)
        // {
        //     IQueryable<Topology> q = _repo.List()
        //         .Where(p => p.Workers.Select(w => w.PersonId).Contains(Profile.Id));

        //     if (search.Term.HasValue())
        //     {
        //         q = q.Where(o => o.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0);
        //     }

        //     return await ProcessQuery(search, q);
        // }

        public async Task<Models.SearchResult<Models.TopologySummary>> ProcessQuery(Models.Search search, IQueryable<Topology> q)
        {
            if (search.Take == 0) search.Take = 50;
            Models.SearchResult<Models.TopologySummary> result = new Models.SearchResult<Models.TopologySummary>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  Mapper.Map<Models.TopologySummary[]>(q
                .OrderBy(t => t.Name)
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArray(), WithActor());
            return result;
        }

        public async Task<Models.SearchResult<Models.Topology>> ListAll(Models.Search search)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            IQueryable<Topology> q = _repo.List()
                .Include(t => t.Templates)
                .Include(t => t.Workers)
                .ThenInclude(w => w.Person);

            if (search.Term.HasValue())
            {
                q = q.Where(o =>
                    o.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || o.Description.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || o.GlobalId.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0
                );
            }

            if (search.Take == 0) search.Take = 50;
            Models.SearchResult<Models.Topology> result = new Models.SearchResult<Models.Topology>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  Mapper.Map<Models.Topology[]>(q
                .OrderBy(t => t.Name)
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
            topo = await _repo.Add(topo);
            topo.Workers.Add(new Worker
            {
                PersonId = Profile.Id,
                Permission = Permission.Manager
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
                    Permission = Permission.Editor
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

            if (!Profile.IsAdmin //if you aren't admin, you can remove the last remaining workspace manager
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

        public async Task<Models.GameState[]> GetGames(int id)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            return Mapper.Map<Models.GameState[]>(topology.Gamespaces);
        }

        public async Task<Models.GameState[]> KillGames(int id)
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

            return Mapper.Map<Models.GameState[]>(result);
        }
    }
}
