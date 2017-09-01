using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Data;
using TopoMojo.Core.Entities;
using TopoMojo.Core.Entities.Extensions;

namespace TopoMojo.Core
{
    public class TopologyManager : EntityManager<Topology>
    {
        public TopologyManager(
            TopoMojoDbContext db,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        ) : base (db, mill, options, profileResolver)
        {
        }

        public new async Task<SearchResult<TopoSummary>> ListAsync(Search search)
        {
            //get topo, workers, templates
            IQueryable<Topology> q = base.ListQuery(search);

            if (search.HasFilter("published"))
                q = q.Where(t => t.IsPublished);

            return await ProcessQuery(search, q);
        }

        public async Task<SearchResult<TopoSummary>> ListMine(Search search)
        {
            IQueryable<Topology> q = _db.Workers
                .Where(p => p.PersonId == _user.Id)
                .Select(p => p.Topology);

            if (search.Term.HasValue())
            {
                q = q.Where(o => o.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            return await ProcessQuery(search, q);
        }

        public async Task<SearchResult<TopoSummary>> ProcessQuery(Search search, IQueryable<Topology> q)
        {
            SearchResult<TopoSummary> result = new SearchResult<TopoSummary>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results =  q
                .Include(t => t.Workers)
                //.Include(t => t.Linkers)
                .OrderBy(t => t.Name)
                .Skip(search.Skip)
                .Take(search.Take)
                .Select(t => new TopoSummary
                {
                    Id = t.Id,
                    GlobalId = t.GlobalId,
                    Name = t.Name,
                    Description = t.Description,
                    IsPublished = t.IsPublished,
                    People = String.Join(", ", t.Workers.Select(p=>p.Person.Name).ToArray()),
                    //Templates = String.Join(" | ", t.Linkers.Select(i=>i.Name).ToArray()),
                    CanManage = _user.IsAdmin || t.Workers.Where(p => p.PersonId == _user.Id && p.Permission != Permission.None).Any()
                })
                .ToArray();
            return result;
        }

        public async Task<Topology> Create(Topology topology)
        {
            if (topology.Id > 0)
                throw new InvalidOperationException();

            topology.Id = 0;
            topology.Workers.Add(new Worker
            {
                PersonId = _user.Id,
                Permission = Permission.Manager
            });
            await base.SaveAsync(topology);
            return topology;
        }

        public async Task<Topology> Update(Topology topo)
        {
            if (! await CanEdit(topo.Id))
                throw new InvalidOperationException();

            topo.Linkers = null;
            topo.Workers = null;
            return await base.SaveAsync(topo);
        }

        public async Task<bool> DeleteAsync(Topology topo)
        {
            if (! await CanEdit(topo.Id))
                throw new InvalidOperationException();

            _db.Topologies.Remove(topo);

            Linker[] list = await _db.Linkers
                .Include(t => t.Template)
                .Where(t => t.TopologyId == topo.Id)
                .ToArrayAsync();
            _db.Linkers.RemoveRange(list);

            foreach (Linker tref in list)
                if (tref.Template.OwnerId == topo.Id)
                    _db.Templates.Remove(tref.Template);

            await _db.SaveChangesAsync();
            return true;
        }

        protected override void Normalize(Topology topo)
        {
            base.Normalize(topo);
        }

        public async Task<bool> AllowedInstanceAccess(string guid)
        {
            Player member = await _db.Players
                .Where(m => m.PersonId == _user.Id
                    && m.Gamespace.GlobalId == guid)
                .SingleOrDefaultAsync();
            return (member != null);
        }

        public async Task<bool> CanEdit(string guid)
        {
            if (_user.IsAdmin)
                return true;

            Worker permission = await _db.Workers
                .Where(p => p.PersonId == _user.Id
                    && p.Topology.GlobalId == guid
                    && p.Permission.HasFlag(Permission.Editor))
                    .SingleOrDefaultAsync();
            return (permission != null);
        }

        public async Task<bool> CanEdit(int topoId)
        {
            if (_user.IsAdmin)
                return true;

            Worker worker = await _db.Workers.Where(w => w.PersonId == _user.Id && w.TopologyId == topoId).SingleOrDefaultAsync();
            return worker != null && worker.CanEdit();
        }

        public async Task<Worker[]> Members(int id)
        {
            if (! await CanEdit(id))
                throw new InvalidOperationException();

            return await _db.Workers
                .Where(m => m.TopologyId == id)
                .Include(m => m.Person)
                .ToArrayAsync();
        }

        // public async Task<Permission> Grant(int id, string users)
        // {
        //     if (!(await Permission(id)).CanManage())
        //         throw new InvalidOperationException();

        //     Topology topo = await _db.Topologies
        //         .Include(t => t.Permissions)
        //         .Where(t => t.Id == id)
        //         .SingleOrDefaultAsync();



        //     Permission member = await _db.Permissions
        //         .Include(p => p.Person)
        //         .Where(p => p.Id == id)
        //         .FirstOrDefaultAsync();

        //     if (member == null)
        //         throw new InvalidOperationException();


        //     member.Value = flag;
        //     _db.Attach(member);
        //     _db.SaveChanges();
        //     return member;
        // }

        // public async Task<Template> AddTemplate(int topoId, int templateId, bool clone)
        // {
        //     Topology topo = await _db.Topologies
        //         .Include(t => t.Templates)
        //         .ThenInclude(tt => tt.Template)
        //         .Where(t => t.Id == topoId)
        //         .SingleOrDefaultAsync();

        //     if (topo == null)
        //         throw new InvalidOperationException();

        //     if (! (await Permission(topoId)).CanEdit())
        //         throw new InvalidOperationException();

        //     Template template = await _db.Templates.FindAsync(templateId);
        //     if (template == null)
        //         throw new InvalidOperationException();

        //     if (clone)
        //     {
        //         Template t = new Template();
        //         t.Name = template.Name;
        //         t.Description = template.Description;
        //         t.GlobalId = Guid.NewGuid().ToString();
        //         t.WhenCreated = DateTime.UtcNow;
        //         TemplateUtility tu = new TemplateUtility(template.Detail);
        //         tu.LocalizeDiskPaths(topo.GlobalId, t.GlobalId);
        //         t.Detail = tu.ToString();
        //         _db.Templates.Add(t);
        //         await _db.SaveChangesAsync();
        //         template = t;
        //     }

        //     topo.Templates.Add( new TopologyTemplate
        //     {
        //         TemplateId = template.Id,
        //         Owned = clone
        //     });
        //     await _db.SaveChangesAsync();

        //     return template;
        // }

        public async Task<Linker[]> ListTemplates(int id)
        {
            return await _db.Linkers
                .Include(tt => tt.Template)
                .Include(tt => tt.Topology)
                .Where(tt => tt.TopologyId == id)
                .ToArrayAsync();
        }

        public async Task<Linker> AddTemplate(Linker tref)
        {
            if (! await CanEdit(tref.TopologyId))
                throw new InvalidOperationException();

            _db.Linkers.Add(tref);
            await _db.SaveChangesAsync();
            await _db.Entry(tref).Reference(t => t.Topology).LoadAsync();
            await _db.Entry(tref).Reference(t => t.Template).LoadAsync();
            tref.Name = tref.Template.Name.ToLower().Replace(" ", "-");
            await _db.SaveChangesAsync();
            return tref;
        }

        public async Task<Linker> UpdateTemplate(Linker tref)
        {
            if (! await CanEdit(tref.TopologyId))
                throw new InvalidOperationException();

            _db.Attach(tref);
            tref.Name = tref.Name.Replace(" ", "-");
            if (tref.Owned)
            {
                tref.Template.Name = tref.Name;
                tref.Template.Description = tref.Description;

                TemplateUtility tu = new TemplateUtility(tref.Template.Detail);
                tu.Name = tref.Name;
                tu.Networks = tref.Networks;
                tu.Iso = tref.Iso;
                tref.Template.Detail = tu.ToString();

                //tref.Name = null;
                tref.Networks = null;
                tref.Iso = null;
                tref.Description = null;
            }
            _db.Update(tref);
            await _db.SaveChangesAsync();
            return tref;
        }

        public async Task<bool> RemoveTemplate(int id)
        {
            Linker tref = await _db.Linkers
                .Include(t => t.Template)
                .Include(t => t.Topology)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();

            if (tref == null)
                throw new InvalidOperationException();

            if (! await CanEdit(tref.TopologyId))
                throw new InvalidOperationException();

            if (tref.Template.OwnerId == tref.TopologyId)
            {
                _db.Remove(tref.Template);
            }
            _db.Remove(tref);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Linker> CloneTemplate(int id)
        {
            Linker tref = await _db.Linkers
                .Include(t => t.Template)
                .Include(t => t.Topology)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();

            if (tref == null)
                throw new InvalidOperationException();

            if (! await CanEdit(tref.TopologyId))
                throw new InvalidOperationException();

            Template template = new Template {
                Name = tref.Name ?? tref.Template.Name,
                Description = tref.Description ?? tref.Template.Description,
                GlobalId = Guid.NewGuid().ToString(),
                WhenCreated = DateTime.UtcNow,
                Detail = tref.Template.Detail,
                OwnerId = tref.TopologyId
            };
            TemplateUtility tu = new TemplateUtility(template.Detail);
            tu.Name = template.Name;
            tu.LocalizeDiskPaths(tref.Topology.GlobalId, template.GlobalId);
            template.Detail = tu.ToString();
            //_db.Templates.Add(template);
            tref.Template = template;
            await _db.SaveChangesAsync();
            //tref.TemplateId = template.Id);
            //await _db.SaveChanges();
            return tref;
        }

        public async Task<string> Share(int id, bool revoke)
        {
            Worker worker = await _db.Workers
                .Include(m => m.Topology)
                .Where(m => m.TopologyId == id && m.PersonId == _user.Id)
                .SingleOrDefaultAsync();

            if (worker == null || worker.PersonId != _user.Id || !worker.CanManage())
                throw new InvalidOperationException();

            string code = (revoke) ? "" : Guid.NewGuid().ToString("N");
            worker.Topology.ShareCode = code;
            await _db.SaveChangesAsync();
            return code;
        }

        public async Task<bool> Publish(int id, bool revoke)
        {
            Worker worker = await _db.Workers
                .Include(m => m.Topology)
                .Where(m => m.TopologyId == id && m.PersonId == _user.Id)
                .SingleOrDefaultAsync();

            if (worker == null || worker.PersonId != _user.Id || !worker.CanManage())
                throw new InvalidOperationException();

            worker.Topology.IsPublished = !revoke;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Enlist(string code)
        {
            Topology workspace = await _db.Topologies
                .Include(i => i.Workers)
                .Where(i => i.ShareCode == code)
                .SingleOrDefaultAsync();

            if (workspace == null)
                throw new InvalidOperationException();

            if (!workspace.Workers.Where(m => m.PersonId == _user.Id).Any())
            {
                workspace.Workers.Add(new Worker
                {
                    PersonId = _user.Id,
                    Permission = Permission.Editor
                });
                await _db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> Delist(int topoId, int memberId)
        {
            Topology topo = await _db.Topologies
                .Include(t => t.Workers)
                .Where(t => t.Id == topoId)
                .SingleOrDefaultAsync();

            Worker actor = topo.Workers
                .Where(p => p.PersonId == _user.Id)
                .SingleOrDefault();

            Worker target = topo.Workers
                .Where(p => p.PersonId == memberId)
                .SingleOrDefault();

            if (actor == null || !actor.CanManage() || target == null)
                throw new InvalidOperationException();

            topo.Workers.Remove(target);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}