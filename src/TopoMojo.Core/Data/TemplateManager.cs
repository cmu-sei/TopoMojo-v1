using System;
using System.Collections.Generic;
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
            IServiceProvider sp
        ) : base (sp)
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

        public async Task<bool> DeleteTemplate(int id)
        {
            if (!_user.IsAdmin)
                throw new InvalidOperationException();

            if (await _db.TTLinkage.Where(t => t.TemplateId == id).AnyAsync())
                throw new InvalidOperationException("Template is linked by others.");

            _db.Templates.Remove(new Template { Id = id });
            await _db.SaveChangesAsync();
            return true;

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
                    && tt.TemplateId == id
                    && tp.Value.HasFlag(PermissionFlag.Editor)
                select p).AnyAsync();
        }

        private async Task<bool> CanEditTopo(int id)
        {
            if (_user.IsAdmin)
                return true;

            return await _db.Permissions
                .Where(p => p.TopologyId == id
                    && p.PersonId == _user.Id
                    && p.Value.HasFlag(PermissionFlag.Editor))
                .AnyAsync();

        }
        public override async Task<SearchResult<Template>> ListAsync(Search search)
        {
            IQueryable<Template> q = ListQuery(search);

            if (search.HasFilter("published"))
                q = q.Where(t => t.IsPublished);

            SearchResult<Template> result = new SearchResult<Template>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results = await q
                .OrderBy(t => t.Name)
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArrayAsync();
            return result;
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

            if (!(await CanEditTopo(tref.TopologyId)))
                throw new InvalidOperationException();

            if (tref.Template.OwnerId == tref.TopologyId)
            {
                _db.Remove(tref.Template);
            }
            _db.Remove(tref);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Models.Template> GetDeployableTemplate(int id, string tag)
        {
            TemplateReference tref = await _db.TTLinkage
                .Include(tt => tt.Template)
                .Include(tt => tt.Topology)
                .Where(tt => tt.Id == id)
                .SingleOrDefaultAsync();

            if (tref == null)
                throw new InvalidOperationException();

            // if (! await CanEdit(tref.TopologyId))
            //     throw new InvalidOperationException();

            TemplateUtility tu = new TemplateUtility(tref.Template.Detail);
            if (tref.Name.HasValue())
                tu.Name = tref.Name;

            if (tref.Networks.HasValue())
                tu.Networks = tref.Networks;

            if (tref.Iso.HasValue())
                tu.Iso = tref.Iso;

            tu.IsolationTag = tag.HasValue() ? tag : tref.Topology.GlobalId;
            tu.Id = tref.Id.ToString();
            return tu.AsTemplate();
        }

    }
}