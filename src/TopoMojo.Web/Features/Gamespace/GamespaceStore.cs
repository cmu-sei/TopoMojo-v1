// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class GamespaceStore : Store<Gamespace>, IGamespaceStore
    {
        public GamespaceStore (
            TopoMojoDbContext db
        ) : base(db) { }

        public override IQueryable<Gamespace> List(string term)
        {
            term = term.ToLower();

            return base.List().Where(g =>
                g.Name.ToLower().Contains(term)
            );
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

        public IQueryable<Gamespace> ListByProfile(string id)
        {
            return DbContext.Players
                .Where(p => p.SubjectId == id)
                .Select(p => p.Gamespace);
        }

        public async Task<Gamespace> Load(int id)
        {
            return await base.Retrieve(id, query => query
                .Include(g => g.Workspace)
                    .ThenInclude(t => t.Templates)
                        .ThenInclude(tm => tm.Parent)
                .Include(g => g.Players)
            );
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

        public async Task<Gamespace> FindByShareCode(string code)
        {
            string id = await DbContext.Gamespaces
                .Where(g => g.ShareCode == code)
                .Select(g => g.GlobalId)
                .SingleOrDefaultAsync();

            return (!string.IsNullOrEmpty(id))
                ? await Retrieve(id)
                : null;
        }

        public async Task<Gamespace[]> ListByContext(string subjectId, string workspaceId)
        {
            return await DbContext.Gamespaces
                .Where(g =>
                    g.Workspace.GlobalId == workspaceId &&
                    g.Players.Any(p => p.SubjectId == subjectId)
                )
                .ToArrayAsync()
            ;
        }

        public async Task<Gamespace> LoadActiveByContext(string subjectId, string workspaceId)
        {
            string id = await DbContext.Gamespaces
                .Where(g =>
                    g.Workspace.GlobalId == workspaceId &&
                    g.StopTime == DateTime.MinValue &&
                    g.Players.Any(p => p.SubjectId == subjectId)
                )
                .Select(p => p.GlobalId)
                .FirstOrDefaultAsync();

            return (!string.IsNullOrEmpty(id))
                ? await Load(id)
                : null;
        }


        public async Task<Gamespace> FindByPlayer(int playerId)
        {
            return null;
            // int id = await DbContext.Players
            //     .Where(p => p.Id == playerId)
            //     .Select(p => p.GamespaceId)
            //     .SingleOrDefaultAsync();

            // return (id > 0)
            //     ? await Retrieve(id)
            //     : null
            // ;
        }

        public async Task<Player[]> LoadPlayers(string id)
        {
            return await DbContext.Players
                .Where(p => p.Gamespace.GlobalId == id)
                .ToArrayAsync()
            ;
        }

        public async Task<bool> CanInteract(string id, string actorId)
        {
            return await DbContext.Gamespaces
                .Where(g =>
                    g.ClientId == actorId ||
                    g.Players.Any(p => p.SubjectId == actorId)
                )
                .AnyAsync()
            ;
        }
    }
}
