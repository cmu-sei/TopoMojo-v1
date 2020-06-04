// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class TemplateStore : DataStore<Template>, ITemplateStore
    {
        public TemplateStore (
            TopoMojoDbContext db,
            IMemoryCache memoryCache,
            IDistributedCache cache
        ) : base(db, memoryCache) { }

        public override IQueryable<Template> List(string term = null)
        {
            return base.List(term)
                .Include(t => t.Workspace);
        }

        public override async Task<Template> Load(int id)
        {
            return await base.Load(id, query => query
                .Include(tt => tt.Parent)
                .Include(tt => tt.Workspace)
                .ThenInclude(t => t.Workers)
            );
        }

        public override async Task<Template> Load(string id)
        {
            return await base.Load(id, query => query
                .Include(tt => tt.Parent)
                .Include(tt => tt.Workspace)
                .ThenInclude(t => t.Workers)
            );
        }

        public async Task<bool> IsParentTemplate(int id)
        {
             return await DbContext.Templates
                .Where(t => t.ParentId == id)
                .AnyAsync();
        }

        public async Task<Template[]> ListChildren(int parentId)
        {
            return await base.List()
                .Include(t => t.Workspace)
                .Where(t => t.ParentId == parentId)
                .ToArrayAsync();
        }

        public async Task<bool> AtTemplateLimit(int topoId)
        {
            Workspace topo = await DbContext.Workspaces.FindAsync(topoId);
            int count = await DbContext.Templates.Where(t => t.WorkspaceId == topoId).CountAsync();
            return count >= topo.TemplateLimit;
        }

        public async Task<string> ResolveKey(string key)
        {
            var name = await DbContext.Workspaces.Where(t => t.GlobalId == key)
                .Select(t => t.Name)
                .SingleOrDefaultAsync();

            if (!string.IsNullOrEmpty(name))
                return "workspace: " + name;

            name = await DbContext.Gamespaces.Where(g => g.GlobalId == key)
                .Select(g => g.Name)
                .SingleOrDefaultAsync();

            if (!string.IsNullOrEmpty(name))
                return "gamespace: " + name;

            return null;
        }
    }
}
