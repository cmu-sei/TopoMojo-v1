using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;

namespace TopoMojo.Core
{
    public class TemplateManager : EntityManager<Template>
    {
        public TemplateManager(
            TopoMojoDbContext db,
            IUserResolver userResolver,
            IOptions<CoreOptions> options,
            ILoggerFactory mill
        ) : base (db, userResolver, options, mill)
        {
        }

        public async Task<TemplateModel> Create(TemplateModel model)
        {
            if (!_user.IsAdmin)
                throw new InvalidOperationException();

            Template t = new Template
            {
                Name = model.Name,
                Description = model.Description,
            };
            await base.SaveAsync(t);
            model.Id = t.Id;
            return model;
        }

        public async Task<Template> Save(Template template)
        {
            if (!_user.IsAdmin)
                throw new InvalidOperationException();

            if (!template.Name.HasValue())
                template.Name = "[TemplateName]";

            TemplateUtility tu = new TemplateUtility(template.Detail);
            tu.Name = template.Name;
            template.Detail = tu.ToString();

            await base.SaveAsync(template);
            return template;
        }

        public async Task<TemplateModel> Update(TemplateModel model)
        {
            Template t = await _db.Templates.FindAsync(model.Id);
            if (t == null)
                throw new InvalidOperationException();

            if (! (await CanEdit(t.Id)))
                throw new InvalidOperationException();

            TemplateUtility tu = new TemplateUtility(t.Detail);
            tu.Name = model.Name;
            tu.Networks = model.Networks;
            tu.Iso = model.Iso;
            t.Detail = tu.ToString();
            await _db.SaveChangesAsync();
            return model;
        }


        private async Task<bool> CanEdit(int id)
        {
            if (_user.IsAdmin)
                return true;

            //if current user is editor of a topo that owns this template
            return await
                (from p in _db.People
                join tp in _db.Permissions on p.Id equals tp.PersonId
                join tt in _db.TTLinkage on tp.TopologyId equals tt.TopologyId
                where p.Id == _user.Id
                    && tp.Value.HasFlag(PermissionFlag.Editor)
                select p).AnyAsync();
        }

        public override async Task<Search<Template>> ListAsync(Search<Template> search)
        {
            IQueryable<Template> q = ListQuery(search);

            if (search.HasFilter("published"))
                q = q.Where(t => t.IsPublished);

            search.Total = await q.CountAsync();
            search.Results = await q.ToArrayAsync();
            return search;
        }

        public async Task<Models.Template> GetDeployableTemplate(int id)
        {
            TemplateReference tref = await _db.TTLinkage
                .Include(tt => tt.Template)
                .Include(tt => tt.Topology)
                .Where(tt => tt.Id == id)
                .SingleOrDefaultAsync();

            if (tref == null)
                throw new InvalidOperationException();

            if (! await CanEdit(tref.TopologyId))
                throw new InvalidOperationException();

            TemplateUtility tu = new TemplateUtility(tref.Template.Detail);
            if (tref.Name.HasValue())
                tu.Name = tref.Name;

            if (tref.Networks.HasValue())
                tu.Networks = tref.Networks;

            if (tref.Iso.HasValue())
                tu.Iso = tref.Iso;

            tu.IsolationTag = tref.Topology.GlobalId;
            tu.Id = tref.Id.ToString();
            return tu.AsTemplate();
        }
    }
}