// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public class TemplateService : EntityService<Data.Template>
    {
        public TemplateService(
            ITemplateStore templateStore,
            IHypervisorService podService,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        ) : base(mill, options, profileResolver)
        {
            _templateStore = templateStore;
            _pod = podService;
        }

        private readonly ITemplateStore _templateStore;
        private readonly IHypervisorService _pod;

        public async Task<Template> Load(int id)
        {
            var template = await _templateStore.Load(id);
            if (template == null)
                throw new InvalidOperationException();

            return Mapper.Map<Template>(template, WithActor());
        }

        public async Task<TemplateDetail> LoadDetail(int id)
        {
            var template = await _templateStore.Load(id);
            if (template == null)
                throw new InvalidOperationException();

            return Mapper.Map<TemplateDetail>(template, WithActor());
        }

        public async Task<TemplateDetail> Create(TemplateDetail model)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            model.Detail = new TemplateUtility("", model.Name).ToString();
            var t = Mapper.Map<Data.Template>(model);
            await _templateStore.Add(t);
            return Mapper.Map<TemplateDetail>(t);
        }

        public async Task<TemplateDetail> Configure(TemplateDetail template)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            var entity = await _templateStore.Load(template.Id);
            if (entity == null)
                throw new InvalidOperationException();

            Mapper.Map<TemplateDetail, Data.Template>(template, entity);
            await _templateStore.Update(entity);
            return Mapper.Map<TemplateDetail>(entity);
        }

        public async Task<Template> Update(ChangedTemplate template)
        {
            var entity = await _templateStore.Load(template.Id);
            if (entity == null)
                throw new InvalidOperationException();

            if (! await _templateStore.CanEdit(template.TopologyId, User))
                throw new InvalidOperationException();

            Mapper.Map<ChangedTemplate, Data.Template>(template, entity);
            await _templateStore.Update(entity);
            return Mapper.Map<Template>(entity);
        }

        public async Task<Template> Link(TemplateLink newlink)
        {
            var entity = await _templateStore.Load(newlink.TemplateId);
            if (entity == null || entity.Parent != null || !entity.IsPublished)
                throw new InvalidOperationException();

            if (await _templateStore.AtTemplateLimit(newlink.TopologyId))
                throw new WorkspaceTemplateLimitException();

            var newTemplate = new Data.Template
            {
                ParentId = entity.Id,
                TopologyId = entity.TopologyId,
                Name = $"{entity.Name}-{new Random().Next(100, 999).ToString()}",
                Description = entity.Description,
                Iso = entity.Iso,
                Networks = entity.Networks
            };
            await _templateStore.Add(newTemplate);
            //TODO: streamline object graph hydration
            newTemplate = await _templateStore.Load(newTemplate.Id);
            return Mapper.Map<Template>(newTemplate, WithActor());
        }

        public async Task<Template> Unlink(TemplateLink link) //CLONE
        {
            var entity = await _templateStore.Load(link.TemplateId);
            if (entity == null)
                throw new InvalidOperationException();

            if (! await _templateStore.CanEdit(entity.TopologyId ?? 0, User))
                throw new InvalidOperationException();

            if (entity.Parent != null)
            {
                TemplateUtility tu = new TemplateUtility(entity.Parent.Detail);
                tu.Name = entity.Name;
                tu.LocalizeDiskPaths(entity.Topology.GlobalId, entity.GlobalId);
                entity.Detail = tu.ToString();
                entity.Parent = null;
                await _templateStore.Update(entity);
            }
            return Mapper.Map<Template>(entity, WithActor());
        }

        public async Task<Template> Delete(int id)
        {
            var entity = await _templateStore.Load(id);
            if (entity == null)
                throw new InvalidOperationException();

            if (! await _templateStore.CanEdit(entity.TopologyId ?? 0, User))
                throw new InvalidOperationException();

            if (await _templateStore.IsParentTemplate(id))
                throw new ParentTemplateException();

            //delete associated vm
            var deployable = await GetDeployableTemplate(id);
            foreach (var vm in await _pod.Find(deployable.Name+"#"+deployable.IsolationTag))
                await _pod.Delete(vm.Id);

            //if root template, delete disk(s)
            if (entity.Parent == null)
                await _pod.DeleteDisks(deployable);

            await _templateStore.Remove(entity);
            return Mapper.Map<Template>(entity);
        }

        public async Task<SearchResult<TemplateSummary>> List(Search search)
        {
            var q = BuildQuery(search);
            var result = new SearchResult<TemplateSummary>();
            if (search.Take == 0) search.Take = 50;
            result.Search = search;
            result.Total = await q.CountAsync();
            result.Results = Mapper.Map<TemplateSummary[]>(await RunQuery(search, q));
            return result;
        }

        private IQueryable<Data.Template> BuildQuery(Search search)
        {
            var q = _templateStore.List();

            if (search.Term.HasValue())
                q = q.Where(t =>
                    t.Name.ToLower().Contains(search.Term.ToLower())
                    // || t.Description.IndexOf(search.Term, StringComparison.OrdinalIgnoreCase) >= 0
                );

            if (search.HasFilter("parents"))
                q = q.Where(t => t.ParentId == null || t.ParentId == 0);

            if (search.HasFilter("published"))
                q = q.Where(t => t.IsPublished);

            return q;
        }

        private async Task<Data.Template[]> RunQuery(Search search, IQueryable<Data.Template> q)
        {
            return await q
                .OrderBy(t => t.Name)
                .Skip(search.Skip)
                .Take(search.Take)
                .ToArrayAsync();
        }

        public async Task<TemplateSummary[]> ListChildTemplates(int templateId)
        {
            var results = await _templateStore.ListLinkedTemplates(templateId);
            return Mapper.Map<TemplateSummary[]>(results);
        }

        public async Task<VmTemplate> GetDeployableTemplate(int id, string tag = "")
        {
            var entity = await _templateStore.Load(id);

            if (entity == null)
                throw new InvalidOperationException();

            return Mapper.Map<ConvergedTemplate>(entity).ToVirtualTemplate();
        }

        public async Task<Dictionary<string, string>> ResolveKeys(string[] keys)
        {
            var map = new Dictionary<string, string>();
            foreach (string key in keys)
            {
                var val = await _templateStore.ResolveKey(key);
                map.Add(key, $"{val ?? "__orphaned"}#{key}");
            }
            return map;
        }
    }
}
