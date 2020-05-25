// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class WorkspaceStore : DataStore<Topology>, IWorkspaceStore
    {
        public WorkspaceStore (
            TopoMojoDbContext db,
            IMemoryCache memoryCache,
            IDistributedCache cache
        ) : base(db, memoryCache) { }

        public override IQueryable<Topology> List(string term = null)
        {
            var q = base.List();

            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToLower();

                q = q.Where(t =>
                    t.Name.ToLower().Contains(term)
                    || t.Description.ToLower().Contains(term)
                );
            }

            return q.Include(t => t.Workers);
        }

        public override async Task<Topology> Load(int id)
        {
            return await base.Load(id, query => query
                .Include(t => t.Templates)
                .Include(t => t.Workers).ThenInclude(w => w.Person)
            );
        }

        public override async Task<Topology> Load(string id)
        {
            return await base.Load(id, query => query
                .Include(t => t.Templates)
                .Include(t => t.Workers).ThenInclude(w => w.Person)
            );
        }

        public async Task<Topology> LoadWithGamespaces(int id)
        {
            return await base.Load(id, query => query
                .Include(t => t.Gamespaces)
            );
        }

        public async Task<Topology> LoadWithParents(int id)
        {
            return await base.Load(id, query => query
                .Include(t => t.Templates)
                .ThenInclude(o => o.Parent)
            );
        }

        public async Task<Topology> FindByShareCode(string code)
        {
            return await DbContext.Topologies
                .Include(t => t.Templates)
                .Include(t => t.Workers).ThenInclude(w => w.Person)
                .Where(t => t.ShareCode == code)
                .SingleOrDefaultAsync();
        }

        public async Task<Topology> FindByWorker(int workerId)
        {
            int id = await DbContext.Workers
                .Where(p => p.Id == workerId)
                .Select(p => p.TopologyId)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Load(id)
                : null;
        }


        public override async Task Delete(int id)
        {
            var workspace = await Load(id);

            var messages = await DbContext.Messages.Where(m => m.RoomId == workspace.GlobalId).ToArrayAsync();

            DbContext.Messages.RemoveRange(messages);

            DbContext.Templates.RemoveRange(workspace.Templates);

            DbContext.Remove(workspace);

            await DbContext.SaveChangesAsync();
        }

        public async Task<int> GetWorkspaceCount(int profileId)
        {
            return await DbContext.Workers
                .CountAsync(w => w.PersonId == profileId && w.Permission.HasFlag(Permission.Manager));
        }

        public async Task<bool> HasGames(int id)
        {
            return await DbContext.Gamespaces
                .AnyAsync(g => g.TopologyId == id);
        }

        public async Task<Topology[]> DeleteStale(DateTime staleAfter, bool published, bool dryrun = true)
        {
            var results = await DbContext.Topologies
                .Where(g =>
                    g.LastActivity < staleAfter
                    && g.IsPublished == published
                ).ToArrayAsync();


            if (!dryrun)
            {
                DbContext.Topologies.RemoveRange(results);

                await DbContext.SaveChangesAsync();
            }

            return results;
        }
    }
}
