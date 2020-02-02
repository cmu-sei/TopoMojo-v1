// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.EntityFrameworkCore
{
    public class GamespaceStore : Store<Gamespace>, IGamespaceStore
    {
        public GamespaceStore (
            TopoMojoDbContext db
        ) : base(db) { }

        public IQueryable<Gamespace> ListByProfile(int id)
        {
            return DbContext.Players
                .Where(p => p.PersonId == id)
                .Select(p => p.Gamespace);
        }

        public override async Task<Gamespace> Load(int id)
        {
            return await DbContext.Gamespaces
                .Include(g => g.Topology)
                    .ThenInclude(t => t.Templates)
                        .ThenInclude(tm => tm.Parent)
                .Include(g => g.Players)
                    .ThenInclude(w => w.Person)
                .Where(g => g.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<Gamespace> FindByShareCode(string code)
        {
            int id = await DbContext.Gamespaces
                .Where(g => g.ShareCode == code)
                .Select(g => g.Id)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Load(id)
                : null;
        }

        public async Task<Gamespace> FindByContext(int topoId, int profileId)
        {
            int id = await DbContext.Players
                .Where(g => g.PersonId == profileId && g.Gamespace.TopologyId == topoId)
                .Select(p => p.GamespaceId)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Load(id)
                : null;
        }

        public async Task<Gamespace> FindByPlayer(int playerId)
        {
            int id = await DbContext.Players
                .Where(p => p.Id == playerId)
                .Select(p => p.GamespaceId)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Load(id)
                : null;
        }

        public override async Task<bool> CanEdit(int entityId, Profile profile)
        {
            if (profile.IsAdmin)
                return true;

            return await DbContext.Players
                .Where(p => p.GamespaceId == entityId
                    && p.PersonId == profile.Id
                    && p.Permission.HasFlag(Permission.Editor))
                .AnyAsync();

        }

        public override async Task<bool> CanManage(int entityId, Profile profile)
        {
            if (profile.IsAdmin)
                return true;

            return await DbContext.Players
                .Where(p => p.GamespaceId == entityId
                    && p.PersonId == profile.Id
                    && p.Permission.HasFlag(Permission.Manager))
                .AnyAsync();

        }

        public IQueryable<Player> ListPlayers(int id)
        {
            return DbContext.Players
                .Include(p => p.Person)
                .Where(p => p.GamespaceId == id);
        }

        public override async Task Remove(Gamespace gamespace)
        {
            //var result = await DbContext.Messages.FromSql($"DELETE FROM Messages WHERE RoomId = '{gamespace.GlobalId}'").FirstOrDefaultAsync();
            var list = await DbContext.Messages.Where(m => m.RoomId == gamespace.GlobalId).ToArrayAsync();
            DbContext.Messages.RemoveRange(list);
            await base.Remove(gamespace);
        }
    }
}
