// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Api.Data.Abstractions;
using TopoMojo.Api.Data.Extensions;
using TopoMojo.Api.Extensions;

namespace TopoMojo.Api.Data
{
    public class GamespaceStore : Store<Gamespace>, IGamespaceStore
    {
        public GamespaceStore (
            TopoMojoDbContext db
        ) : base(db) { }

        public override IQueryable<Gamespace> List(string term)
        {
            term = term.ToLower();

            return base.List()
                .Where(g =>
                    g.Name.ToLower().Contains(term)
                )
            ;
        }

        public IQueryable<Gamespace> ListByUser(string subjectId)
        {
            return base.List()
                .Where(g =>
                    g.ManagerId == subjectId ||
                    g.Players.Any(p => p.SubjectId == subjectId)
                )
            ;
        }

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

        public async Task<Gamespace> Load(string id)
        {
            return await base.Retrieve(id, query => query
                .Include(g => g.Workspace)
                    .ThenInclude(t => t.Templates)
                        .ThenInclude(tm => tm.Parent)
                .Include(g => g.Players)
            );
        }

        public async Task<Gamespace> LoadActiveByContext(string subjectId, string workspaceId)
        {
            string id = await DbSet.Where(g =>
                    g.WorkspaceId == workspaceId &&
                    g.EndTime == DateTime.MinValue &&
                    g.Players.Any(p => p.SubjectId == subjectId)
                )
                .Select(p => p.Id)
                .FirstOrDefaultAsync()
            ;

            return (!string.IsNullOrEmpty(id))
                ? await Load(id)
                : null;
        }

        public async Task<Player[]> LoadPlayers(string id)
        {
            return await DbContext.Players
                .Where(p => p.Gamespace.Id == id)
                .ToArrayAsync()
            ;
        }

        public async Task<bool> CanInteract(string id, string actorId)
        {
            return await DbSet.AnyAsync(g =>
                g.Id == id &&
                (
                    g.ManagerId == actorId ||
                    g.Players.Any(p => p.SubjectId == actorId)
                )
            );
        }

        public async Task<bool> CanManage(string id, string subjectId)
        {
            var gamespace = await DbSet.FindAsync(id);

            return gamespace.ManagerId.Equals(subjectId);
        }

        public async Task<bool> HasValidUserScope(string workspaceId, string scope)
        {
            var workspace = await DbContext.Workspaces.FindAsync(workspaceId);

            return workspace.Audience.HasAnyToken(scope);
        }

        public async Task<bool> IsBelowGamespaceLimit(string subjectId, int limit)
        {
            return
                limit == 0 ||
                limit > await DbSet.CountAsync(g => g.ManagerId == subjectId)
            ;
        }

        public async Task<Player> FindPlayer(string id, string subjectId)
        {
            return await DbContext.Players.FindAsync(subjectId, id);
        }

        public async Task DeletePlayer(string id, string subjectId)
        {
            var player = await FindPlayer(id, subjectId);

            if (player is Data.Player)
            {
                DbContext.Players.Remove(player);

                await DbContext.SaveChangesAsync();
            }
        }
    }
}
