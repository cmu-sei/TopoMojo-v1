using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Core.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;

namespace TopoMojo.Core
{
    public class TemplateManager : EntityManager<Template>
    {
        public TemplateManager(
            IProfileRepository profileRepository,
            ITemplateRepository repo,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        ) : base(profileRepository, mill, options, profileResolver)
        {
            _repo = repo;
        }

        private readonly ITemplateRepository _repo;

        public async Task<Models.TemplateDetail> Create(Models.NewTemplateDetail model)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            Template t = Mapper.Map<Template>(model);
            await _repo.Add(t);
            return Mapper.Map<Models.TemplateDetail>(t);
        }

        public async Task<Models.TemplateDetail> Configure(Models.TemplateDetail template)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            Template entity = await _repo.Load(template.Id);
            if (entity == null)
                throw new InvalidOperationException();

            Mapper.Map<Models.TemplateDetail, Template>(template, entity);
            await _repo.Update(entity);
            return Mapper.Map<Models.TemplateDetail>(entity);
        }

        public async Task<Models.Template> Update(Models.ChangedTemplate template)
        {
            Template entity = await _repo.Load(template.Id);
            if (entity == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(template.Id, Profile))
                throw new InvalidOperationException();

            Mapper.Map<Models.ChangedTemplate, Template>(template, entity);
            await _repo.Update(entity);
            return Mapper.Map<Models.Template>(entity);
        }

        public async Task<Models.Template> Link(int templateId, int topoId)
        {
            Template entity = await _repo.Load(templateId);
            if (entity == null || entity.Parent != null || !entity.IsPublished)
                throw new InvalidOperationException();

            //assuming topo exists; EF will throw if nonexistent
            Template linked = Mapper.Map<Template>(entity);
            linked.TopologyId = topoId;
            // linked.Id = 0;
            // linked.Detail = "";
            // linked.IsPublished = false;
            // linked.ParentId = entity.Id;
            await _repo.Add(linked);
            return Mapper.Map<Models.Template>(linked, WithActor());
        }

        public async Task<Models.Template> Unlink(int id)
        {
            Template entity = await _repo.Load(id);
            if (entity == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(entity.TopologyId ?? 0, Profile))
                throw new InvalidOperationException();

            if (entity.Parent != null)
            {
                TemplateUtility tu = new TemplateUtility(entity.Parent.Detail);
                tu.Name = entity.Name;
                tu.LocalizeDiskPaths(entity.Topology.GlobalId, entity.GlobalId);
                entity.Detail = tu.ToString();
                entity.Parent = null;
                await _repo.Update(entity);
            }
            return Mapper.Map<Models.Template>(entity, WithActor());
        }

        public async Task<Models.Template> Delete(int id)
        {
            Template entity = await _repo.Load(id);
            if (entity == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(entity.TopologyId ?? 0, Profile))
                throw new InvalidOperationException();

            if (! await _repo.IsParentTemplate(id))
                throw new ParentTemplateException();

            await _repo.Remove(entity);
            return Mapper.Map<Models.Template>(entity);
        }

        public async Task<SearchResult<Models.TemplateSummary>> List(Search search)
        {
            IQueryable<Template> q = BuildQuery(search);
            SearchResult<Models.TemplateSummary> result = new SearchResult<Models.TemplateSummary>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results = Mapper.Map<Models.TemplateSummary[]>(await RunQuery(search, q));
            return result;
        }

        public async Task<SearchResult<Models.TemplateDetail>> ListDetail(Search search)
        {
            IQueryable<Template> q = BuildQuery(search);
            SearchResult<Models.TemplateDetail> result = new SearchResult<Models.TemplateDetail>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results = Mapper.Map<Models.TemplateDetail[]>(await RunQuery(search, q));
            return result;
        }

        private IQueryable<Template> BuildQuery(Search search)
        {
            IQueryable<Template> q = _repo.List();

            if (search.HasFilter("parents"))
                q = q.Where(t => t.ParentId == 0);

            if (search.HasFilter("published"))
                q = q.Where(t => t.IsPublished);

            return q;
        }

        private async Task<Template[]> RunQuery(Search search, IQueryable<Template> q)
        {
            return await q
                .OrderBy(t => t.Name)
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArrayAsync();
        }

        public async Task<Models.TemplateSummary[]> ListChildTemplates(int templateId)
        {
            var results = await _repo.ListLinkedTemplates(templateId);
            return Mapper.Map<Models.TemplateSummary[]>(results);
        }

        public async Task<TopoMojo.Models.Virtual.Template> GetDeployableTemplate(int id, string tag)
        {
            Template template = await _repo.Load(id);

            if (template == null)
                throw new InvalidOperationException();

            TemplateUtility tu = new TemplateUtility(template.Detail ?? template.Parent.Detail);
            tu.Name = template.Name;
            tu.Networks = template.Networks;
            tu.Iso = template.Iso;
            tu.IsolationTag = tag.HasValue() ? tag : template.Topology.GlobalId;
            tu.Id = template.Id.ToString();
            return tu.AsTemplate();
        }

    }
}