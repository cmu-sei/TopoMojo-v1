// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;

namespace TopoMojo.Data
{
    public class TemplateStore : Store<Template>, ITemplateStore
    {
        public TemplateStore (
            TopoMojoDbContext db
        ) : base(db) { }

        public override IQueryable<Template> List(string term = null)
        {
            return term.IsEmpty()
                ? base.List()
                : base.List().Where(t =>
                    t.Name.ToLower().Contains(term)
                );
        }

        public async Task<Template> Load(int id)
        {
            return await base.Retrieve(id, query => query
                .Include(tt => tt.Parent)
                .Include(tt => tt.Workspace)
                .ThenInclude(t => t.Workers)
            );
        }

        public async Task<Template> Load(string id)
        {
            return await base.Retrieve(id, query => query
                .Include(tt => tt.Parent)
                .Include(tt => tt.Workspace)
                .ThenInclude(t => t.Workers)
            );
        }

        public async Task<bool> HasDescendents(string id)
        {
            // var entity = await Retrieve(id);

            return await DbContext.Templates
                .Where(t => t.ParentGlobalId == id)
                .AnyAsync();
        }

        public async Task<Template[]> ListChildren(string parentId)
        {
            return await base.List()
                .Include(t => t.Workspace)
                .Where(t => t.ParentGlobalId == parentId)
                .ToArrayAsync();
        }

        public async Task<bool> AtTemplateLimit(string id)
        {
            return await DbContext.Workspaces
                .Where(w => w.GlobalId == id)
                .Select(w => w.TemplateLimit >= w.Templates.Count)
                .FirstOrDefaultAsync();
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
