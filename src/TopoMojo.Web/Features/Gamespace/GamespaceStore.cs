// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
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
    public class GamespaceStore : DataStore<Gamespace>, IGamespaceStore
    {
        public GamespaceStore (
            TopoMojoDbContext db,
            IMemoryCache memoryCache,
            IDistributedCache cache = null
        ) : base(db, memoryCache) { }

        public override async Task<Gamespace> Create(Gamespace entity)
        {

            if (entity.Workspace != null)
            {
                entity.Workspace.LaunchCount += 1;

                entity.Workspace.LastActivity = DateTime.UtcNow;
            }

            var gamespace = await base.Create(entity);

            return gamespace;
        }

        public IQueryable<Gamespace> ListByProfile(string id)
        {
            return DbContext.Players
                .Where(p => p.SubjectId == id)
                .Select(p => p.Gamespace);
        }

        public override async Task<Gamespace> Retrieve(int id)
        {
            return await base.Retrieve(id, query => query
                .Include(g => g.Workspace)
                    .ThenInclude(t => t.Templates)
                        .ThenInclude(tm => tm.Parent)
                .Include(g => g.Players)
            );
        }

        public override async Task<Gamespace> Retrieve(string id)
        {
            return await base.Retrieve(id, query => query
                .Include(g => g.Workspace)
                    .ThenInclude(t => t.Templates)
                        .ThenInclude(tm => tm.Parent)
                .Include(g => g.Players)
            );
        }

        public async Task<Gamespace> FindByShareCode(string code)
        {
            int id = await DbContext.Gamespaces
                .Where(g => g.ShareCode == code)
                .Select(g => g.Id)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Retrieve(id)
                : null;
        }

        public async Task<Gamespace> FindByContext(int workspaceId, string subjectId)
        {
            int id = await DbContext.Players
                .Where(g => g.SubjectId == subjectId && g.Gamespace.WorkspaceId == workspaceId)
                .Select(p => p.GamespaceId)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Retrieve(id)
                : null;
        }

        public async Task<Gamespace> FindByContext(string subjectId, string workspaceId)
        {
            int id = await DbContext.Players
                .Where(g => g.SubjectId == subjectId && g.WorkspaceId == workspaceId)
                .Select(p => p.GamespaceId)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Retrieve(id)
                : null;
        }

        public async Task<Gamespace> FindByPlayer(int playerId)
        {
            int id = await DbContext.Players
                .Where(p => p.Id == playerId)
                .Select(p => p.GamespaceId)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Retrieve(id)
                : null;
        }

        public override async Task Delete(int id)
        {
            var entity = await base.Retrieve(id);
            var list = await DbContext.Messages.Where(m => m.RoomId == entity.GlobalId).ToArrayAsync();
            DbContext.Messages.RemoveRange(list);
            await base.Delete(id);
        }

    }
}
