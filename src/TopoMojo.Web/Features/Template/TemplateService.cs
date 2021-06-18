// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class TemplateService : _Service
    {
        public TemplateService(
            ITemplateStore templateStore,
            IHypervisorService podService,
            ILogger<TemplateService> logger,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base(logger, mapper, options, identityResolver)
        {
            _templateStore = templateStore;

            _pod = podService;
        }

        private readonly ITemplateStore _templateStore;
        private readonly IHypervisorService _pod;

        public async Task<TemplateSummary[]> List(TemplateSearch search, bool sudo, CancellationToken ct = default(CancellationToken))
        {
            var q = _templateStore.List(search.Term)
                .Include(t => t.Workspace) as IQueryable<Data.Template>;

            if (sudo)
                if (search.pid.NotEmpty())
                    q = q.Where(t => t.ParentGlobalId == search.pid);
                else
                    q = q.Where(t => t.ParentGlobalId == null);
                    // q = q.Where(t => t.ParentId == null || t.ParentId == 0);

            if (!sudo || search.WantsPublished)
                q = q.Where(t => t.IsPublished);

            q = q.OrderBy(t => t.Name);

            if (search.Skip > 0)
                q = q.Skip(search.Skip);

            if (search.Take > 0)
                q = q.Take(search.Take);

            return Mapper.Map<TemplateSummary[]>(
                await q.ToArrayAsync(ct)
            );
        }

        public async Task<Template> Load(string id)
        {
            var template = await _templateStore.Retrieve(id);

            return Mapper.Map<Template>(template);
        }

        internal async Task<bool> CanEdit(string id, string actorId)
        {
            return await _templateStore.DbContext.Templates
                .Where(t => t.GlobalId == id)
                .SelectMany(t => t.Workspace.Workers)
                .AnyAsync(w => w.SubjectId == actorId);
        }

        internal async Task<bool> CanEditWorkspace(string id, string actorId)
        {
            return await _templateStore.DbContext.Workspaces
                .Where(t => t.GlobalId == id)
                .SelectMany(w => w.Workers)
                .AnyAsync(w => w.SubjectId == actorId);
        }

        public async Task<TemplateDetail> LoadDetail(string id)
        {
            var template = await _templateStore.Retrieve(id);

            return Mapper.Map<TemplateDetail>(template);
        }

        public async Task<TemplateDetail> Create(TemplateDetail model)
        {
            model.Detail = new TemplateUtility(model.Detail, model.Name).ToString();

            var t = Mapper.Map<Data.Template>(model);

            await _templateStore.Create(t);

            return Mapper.Map<TemplateDetail>(t);
        }

        public async Task<TemplateDetail> Configure(TemplateDetail template)
        {
            var entity = await _templateStore.Retrieve(template.GlobalId);

            Mapper.Map<TemplateDetail, Data.Template>(template, entity);

            await _templateStore.Update(entity);

            return Mapper.Map<TemplateDetail>(entity);
        }

        public async Task<Template> Update(ChangedTemplate template)
        {
            var entity = await _templateStore.Retrieve(template.GlobalId);

            Mapper.Map<ChangedTemplate, Data.Template>(template, entity);

            await _templateStore.Update(entity);

            return Mapper.Map<Template>(entity);
        }

        public async Task<Template> Link(TemplateLink newlink, bool sudo)
        {
            var entity = await _templateStore.Retrieve(newlink.TemplateId);

            if (entity.IsPublished.Equals(false))
                throw new TemplateNotPublished();

            if (!sudo && await _templateStore.AtTemplateLimit(newlink.WorkspaceId))
                throw new TemplateLimitReached();

            var workspace = await _templateStore.DbContext.Workspaces
                .FirstOrDefaultAsync(w => w.GlobalId == newlink.WorkspaceId)
            ;

            var newTemplate = new Data.Template
            {
                ParentGlobalId = entity.GlobalId,
                WorkspaceGlobalId = workspace.GlobalId,
                Name = $"{entity.Name}-{new Random().Next(100, 999).ToString()}",
                Description = entity.Description,
                Iso = entity.Iso,
                Networks = entity.Networks,
                Guestinfo = entity.Guestinfo
            };

            await _templateStore.Create(newTemplate);

            return Mapper.Map<Template>(
                await _templateStore.Load(newTemplate.GlobalId)
            );
        }

        public async Task<Template> Unlink(TemplateLink link) //CLONE
        {
            var entity = await _templateStore.Retrieve(link.TemplateId);

            if (entity.IsLinked)
            {
                TemplateUtility tu = new TemplateUtility(entity.Parent.Detail);

                tu.Name = entity.Name;

                tu.LocalizeDiskPaths(entity.Workspace.GlobalId, entity.GlobalId);

                entity.Detail = tu.ToString();

                entity.Parent = null;

                await _templateStore.Update(entity);
            }

            return Mapper.Map<Template>(
                await _templateStore.Load(link.TemplateId)
            );
        }

        public async Task<Template> Delete(string id)
        {
            var entity = await _templateStore.Retrieve(id);

            if (await _templateStore.HasDescendents(id))
                throw new TemplateHasDescendents();

            // delete associated vm
            var deployable = await GetDeployableTemplate(id);

            await _pod.DeleteAll($"{deployable.Name}#{deployable.IsolationTag}");

            // if root template, delete disk(s)
            if (entity.IsLinked.Equals(false))
                await _pod.DeleteDisks(deployable);

            await _templateStore.Delete(id);

            return Mapper.Map<Template>(entity);
        }

        public async Task<VmTemplate> GetDeployableTemplate(string id, string tag = "")
        {
            var entity = await _templateStore.Load(id);

            return Mapper.Map<ConvergedTemplate>(entity).ToVirtualTemplate();
        }

        public async Task<Dictionary<string, string>> ResolveKeys(string[] keys)
        {
            var map = new Dictionary<string, string>();

            foreach (string key in keys.Distinct())
            {
                var val = await _templateStore.ResolveKey(key);

                map.Add(key, $"{val ?? "__orphaned"}#{key}");
            }

            return map;
        }
    }
}
