using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Core.Abstractions;
using TopoMojo.Data;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;
using TopoMojo.Data.Entities.Extensions;
using TopoMojo.Core.Models;

namespace TopoMojo.Core
{
    public class TemplateManager : EntityManager<Data.Entities.Template>
    {
        public TemplateManager(
            //TopoMojoDbContext db,
            IProfileRepository profileRepository,
            ITemplateRepository repo,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        ) : base(profileRepository, mill, options, profileResolver)
        {
            _repo = repo;
            Mapper.Initialize(config =>
                config.CreateMap<Models.Template, Data.Entities.Template>()
            );
        }

        private readonly ITemplateRepository _repo;

        public async Task<Models.Template> Create(Models.Template model)
        {

            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            Data.Entities.Template t = new Data.Entities.Template
            {
                Name = model.Name,
                Description = model.Description,
                Networks = "lan"
            };

            t = await _repo.Add(t);
            model.Id = t.Id;
            return model;
        }

        public async Task<Models.Template> Update(Models.Template template)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            Data.Entities.Template entity = await _repo.Load(template.Id);
            if (entity == null)
                throw new InvalidOperationException();

            if (!template.Name.HasValue())
                template.Name = "[TemplateName]";

            entity = Mapper.Map<Models.Template, Data.Entities.Template>(template, entity);
            await _repo.Update(entity);
            return template;
        }

        public async Task<Models.Template> Delete(int id)
        {
            Data.Entities.Template entity = await _repo.Load(id);

            if (entity == null)
                throw new InvalidOperationException();

            if (! await CanEdit(entity.TopologyId ?? 0))
                throw new InvalidOperationException();

            if (! await _repo.HasLinkedTemplates(id))
                throw new InvalidOperationException("Template is linked by others.");

            await _repo.Remove(entity);
            return Mapper.Map<Models.Template>(entity);
        }

        private async Task<bool> CanEdit(int topoId)
        {
            return  await _repo.CanEdit(topoId, Profile);
        }

        public async Task<SearchResult<Models.Template>> ListAsync(Search search)
        {
            IQueryable<Data.Entities.Template> q = _repo.List();

            if (search.HasFilter("published"))
                q = q.Where(t => t.IsPublished);

            SearchResult<Models.Template> result = new SearchResult<Models.Template>();
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results = await q
                .OrderBy(t => t.Name)
                .Skip(search.Skip)
                .Take(search.Take)
                .Select(t => Mapper.Map<Models.Template>(t))
                .ToArrayAsync();
            return result;
        }

        private async Task<Models.LinkedTemplate[]> GetLinkedTemplates(int templateId)
        {
            var results = await _repo.ListLinkedTemplates(templateId);
            return results
                .Select(t => Mapper.Map<Models.LinkedTemplate>(t))
                .ToArray();
        }


        public async Task<TopoMojo.Models.Virtual.Template> GetDeployableTemplate(int id, string tag)
        {
            Data.Entities.Template template = await _repo.Load(id);

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

        public async Task<Models.Template> Clone(int id)
        {
            Data.Entities.Template template = await _repo.Load(id);
            if (template == null)
                throw new InvalidOperationException();

            if (! await CanEdit(template.TopologyId ?? 0))
                throw new InvalidOperationException();

            if (template.Parent != null)
            {
                TemplateUtility tu = new TemplateUtility(template.Parent.Detail);
                tu.Name = template.Name;
                tu.LocalizeDiskPaths(template.Topology.GlobalId, template.GlobalId);
                template.Detail = tu.ToString();
                template.Parent = null;
                await _repo.Update(template);
            }
            return Mapper.Map<Models.Template>(template);
        }
    }
}