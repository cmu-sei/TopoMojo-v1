using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;

namespace TopoMojo.Core
{
    public class TopologyManager : EntityManager<Topology>
    {
        public TopologyManager(
            TopoMojoDbContext db,
            IUserResolver userResolver,
            IOptions<CoreOptions> options,
            ILoggerFactory mill
        ) : base (db, userResolver, options, mill)
        {
        }

        public async Task<Search<TopoSummary>> ListAsync(Search<TopoSummary> search)
        {
            //get topo, contributors, templates
            IQueryable<Topology> q = base.ListQuery(search);
            search.Total = await q.CountAsync();
            search.Results =  q
                .Include(t => t.Permissions)
                .Include(t => t.Templates)
                .OrderBy(t => t.Name)
                .Skip(search.Skip)
                .Take(search.Take)
                .Select(t => new TopoSummary
                {
                    Id = t.Id,
                    GlobalId = t.GlobalId,
                    Name = t.Name,
                    Description = t.Description,
                    People = String.Join(", ", t.Permissions.Select(p=>p.Person.Name).ToArray()),
                    Templates = String.Join(" | ", t.Templates.Select(i=>i.Name).ToArray()),
                    CanManage = _user.IsAdmin || t.Permissions.Where(p => p.PersonId == _user.Id && p.Value != PermissionFlag.None).Any()
                })
                .ToArray();
            return search;
        }

        public async Task<Topology> Create(Topology topology)
        {
            if (topology.Id > 0)
                throw new InvalidOperationException();

            topology.Id = 0;
            topology.Permissions.Add(new Permission {
                PersonId = _user.Id,
                Value = PermissionFlag.Manager
            });
            await base.SaveAsync(topology);
            // _db.Permissions.Add(new Permission {
            //     EntityType = EntityType.Topology,
            //     EntityId = topology.Id,
            //     PersonId = _user.Id,
            //     Value = PermissionFlag.Manager
            // });
            //_db.SaveChanges();
            return topology;
        }

        public async Task<Topology> Update(Topology topo)
        {
            if (!(await Permission(topo.Id)).CanEdit())
                throw new InvalidOperationException();

            topo.Templates = null;
            return await base.SaveAsync(topo);
        }

        public async Task<bool> DeleteAsync(Topology topo)
        {
            if (! await CanEdit(topo.Id))
                throw new InvalidOperationException();

            _db.Topologies.Remove(topo);

            TemplateReference[] list = await _db.TTLinkage
                .Include(t => t.Template)
                .Where(t => t.TopologyId == topo.Id)
                .ToArrayAsync();
            _db.TTLinkage.RemoveRange(list);

            foreach (TemplateReference tref in list)
                if (tref.Template.OwnerId == topo.Id)
                    _db.Templates.Remove(tref.Template);

            await _db.SaveChangesAsync();
            return true;
        }

        protected override void Normalize(Topology topo)
        {
            base.Normalize(topo);
        }

        public async Task<bool> CanEdit(string guid)
        {
            if (_user.IsAdmin)
                return true;

            Permission permission = await _db.Permissions
                .Where(p => p.PersonId == _user.Id
                    && p.Topology.GlobalId == guid
                    && p.Value.HasFlag(PermissionFlag.Editor))
                    .SingleOrDefaultAsync();
            return (permission != null);
        }

        public async Task<bool> CanEdit(int id)
        {
            return (await Permission(id)).CanEdit();
        }

        private async Task<PermissionFlag> Permission(int topoId)
        {
            return _user.IsAdmin
                ? PermissionFlag.Manager
                : await _db.Permissions.PermissionFor(_user.Id, topoId, EntityType.Topology);
        }

        public async Task<Permission[]> Members(int id)
        {
            if (! (await Permission(id)).CanManage())
                throw new InvalidOperationException();

            return await _db.Permissions
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

        public async Task<TemplateReference[]> ListTemplates(int id)
        {
            return await _db.TTLinkage
                .Include(tt => tt.Template)
                .Where(tt => tt.TopologyId == id)
                .ToArrayAsync();
        }

        public async Task<TemplateReference> AddTemplate(TemplateReference tref)
        {
            if (!(await Permission(tref.TopologyId)).CanEdit())
                throw new InvalidOperationException();

            _db.TTLinkage.Add(tref);
            await _db.SaveChangesAsync();
            await _db.Entry(tref).Reference(t => t.Template).LoadAsync();
            tref.Name = tref.Template.Name.Replace(" ", "-");
            await _db.SaveChangesAsync();
            return tref;
        }

        public async Task<TemplateReference> UpdateTemplate(TemplateReference tref)
        {
            if (!(await Permission(tref.TopologyId)).CanEdit())
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
            TemplateReference tref = await _db.TTLinkage
                .Include(t => t.Template)
                .Include(t => t.Topology)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();

            if (tref == null)
                throw new InvalidOperationException();

            if (!(await Permission(tref.TopologyId)).CanEdit())
                throw new InvalidOperationException();

            if (tref.Template.OwnerId == tref.TopologyId)
            {
                _db.Remove(tref.Template);
            }
            _db.Remove(tref);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TemplateReference> CloneTemplate(int id)
        {
            TemplateReference tref = await _db.TTLinkage
                .Include(t => t.Template)
                .Include(t => t.Topology)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();

            if (tref == null)
                throw new InvalidOperationException();

            if (!(await Permission(tref.TopologyId)).CanEdit())
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
    }
}