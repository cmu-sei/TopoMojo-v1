using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
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
            IProfileRepository profileRepository,
            ITopologyRepository repo,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            IPodManager podManager
        ) : base (profileRepository, mill, options, profileResolver)
        {
            _repo = repo;
            _pod = podManager;
        }

        private readonly ITopologyRepository _repo;
        private readonly IPodManager _pod;

        public async Task<SearchResult<Models.Topology>> List(Search search)
        {
            IQueryable<Topology> q = _repo.List();
            if (search.Term.HasValue())
            {
                q = q.Where(o => o.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0);
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

        public async Task<SearchResult<Models.Topology>> ProcessQuery(Search search, IQueryable<Topology> q)
        {
            SearchResult<Models.Topology> result = new SearchResult<Models.Topology>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  Mapper.Map<Models.Topology[]>(q.OrderBy(t => t.Name).Skip(search.Skip).Take(search.Take).ToArray(), WithActor());
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
            Data.Entities.Topology topo = Mapper.Map<Data.Entities.Topology>(model);
            topo.Workers.Add(new Worker
            {
                Person = Profile,
                Permission = Permission.Manager
            });
            topo = await _repo.Add(topo);
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

        public async Task<Models.Topology> DeleteAsync(int id)
        {
            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidOperationException();

            foreach (Vm vm in await _pod.Find(topology.GlobalId))
                _pod.Delete(vm.Id);

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

        // public async Task<Worker[]> Members(int id)
        // {
        //     if (! await CanEdit(id))
        //         throw new InvalidOperationException();

        //     return await _db.Workers
        //         .Where(m => m.TopologyId == id)
        //         .Include(m => m.Person)
        //         .ToArrayAsync();
        // }

        // public async Task<Linker[]> ListTemplates(int id)
        // {
        //     return await _db.Linkers
        //         .Include(tt => tt.Template)
        //         .Include(tt => tt.Topology)
        //         .Where(tt => tt.TopologyId == id)
        //         .ToArrayAsync();
        // }

        // public async Task<Linker> AddTemplate(Linker tref)
        // {
        //     if (! await CanEdit(tref.TopologyId))
        //         throw new InvalidOperationException();

        //     _db.Linkers.Add(tref);
        //     await _db.SaveChangesAsync();
        //     await _db.Entry(tref).Reference(t => t.Topology).LoadAsync();
        //     await _db.Entry(tref).Reference(t => t.Template).LoadAsync();
        //     tref.Name = tref.Template.Name.ToLower().Replace(" ", "-");
        //     await _db.SaveChangesAsync();
        //     return tref;
        // }

        // public async Task<Linker> UpdateTemplate(Linker tref)
        // {
        //     if (! await CanEdit(tref.TopologyId))
        //         throw new InvalidOperationException();

        //     _db.Attach(tref);
        //     tref.Name = tref.Name.Replace(" ", "-");
        //     if (tref.Owned)
        //     {
        //         tref.Template.Name = tref.Name;
        //         tref.Template.Description = tref.Description;

        //         TemplateUtility tu = new TemplateUtility(tref.Template.Detail);
        //         tu.Name = tref.Name;
        //         tu.Networks = tref.Networks;
        //         tu.Iso = tref.Iso;
        //         tref.Template.Detail = tu.ToString();

        //         //tref.Name = null;
        //         tref.Networks = null;
        //         tref.Iso = null;
        //         tref.Description = null;
        //     }
        //     _db.Update(tref);
        //     await _db.SaveChangesAsync();
        //     return tref;
        // }

        // public async Task<bool> RemoveTemplate(int id)
        // {
        //     Linker tref = await _db.Linkers
        //         .Include(t => t.Template)
        //         .Include(t => t.Topology)
        //         .Where(t => t.Id == id)
        //         .SingleOrDefaultAsync();

        //     if (tref == null)
        //         throw new InvalidOperationException();

        //     if (! await CanEdit(tref.TopologyId))
        //         throw new InvalidOperationException();

        //     if (tref.Template.OwnerId == tref.TopologyId)
        //     {
        //         _db.Remove(tref.Template);
        //     }
        //     _db.Remove(tref);
        //     await _db.SaveChangesAsync();
        //     return true;
        // }

        // public async Task<Linker> CloneTemplate(int id)
        // {
        //     Linker tref = await _db.Linkers
        //         .Include(t => t.Template)
        //         .Include(t => t.Topology)
        //         .Where(t => t.Id == id)
        //         .SingleOrDefaultAsync();

        //     if (tref == null)
        //         throw new InvalidOperationException();

        //     if (! await CanEdit(tref.TopologyId))
        //         throw new InvalidOperationException();

        //     Template template = new Template {
        //         Name = tref.Name ?? tref.Template.Name,
        //         Description = tref.Description ?? tref.Template.Description,
        //         GlobalId = Guid.NewGuid().ToString(),
        //         WhenCreated = DateTime.UtcNow,
        //         Detail = tref.Template.Detail,
        //         OwnerId = tref.TopologyId
        //     };
        //     TemplateUtility tu = new TemplateUtility(template.Detail);
        //     tu.Name = template.Name;
        //     tu.LocalizeDiskPaths(tref.Topology.GlobalId, template.GlobalId);
        //     template.Detail = tu.ToString();
        //     //_db.Templates.Add(template);
        //     tref.Template = template;
        //     await _db.SaveChangesAsync();
        //     //tref.TemplateId = template.Id);
        //     //await _db.SaveChanges();
        //     return tref;
        // }

        public async Task<Models.TopologyState> Share(int id, bool revoke)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidTimeZoneException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            topology.ShareCode = (revoke) ? "" : Guid.NewGuid().ToString("N");
            await _repo.Update(topology);
            return Mapper.Map<Models.TopologyState>(topology);
        }

        public async Task<Models.TopologyState> Publish(int id, bool revoke)
        {
            Data.Entities.Topology topology = await _repo.Load(id);
            if (topology == null)
                throw new InvalidTimeZoneException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            topology.IsPublished = !revoke;
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

        public async Task<bool> Delist(int topoId, int memberId)
        {
            Topology topology = await _repo.Load(topoId);

            if (topology == null)
                throw new InvalidOperationException();

            if (! await _repo.CanManage(topoId, Profile))
                throw new InvalidOperationException();

            Worker member = topology.Workers
                .Where(p => p.PersonId == memberId)
                .SingleOrDefault();

            if (member != null)
            {
                topology.Workers.Remove(member);
                await _repo.Update(topology);
            }
            return true;
        }
    }
}