using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.EntityFrameworkCore
{
    public class TopologyRepository : Repository<Topology>, ITopologyRepository
    {
        public TopologyRepository (
            TopoMojoDbContext db
        ) : base(db) { }

        public override IQueryable<Topology> List()
        {
            return DbContext.Topologies
                .Include(t => t.Workers)
                .ThenInclude(w => w.Person);
        }

        public override async Task<Topology> Load(int id)
        {
            return await DbContext.Topologies
                .Include(t => t.Templates)
                .Include(t => t.Workers).ThenInclude(w => w.Person)
                .Include(t => t.Gamespaces)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<Topology> FindByShareCode(string code)
        {
            return await DbContext.Topologies
                .Include(t => t.Templates)
                .Include(t => t.Workers)
                .Include(t => t.Gamespaces)
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

        public override async Task<bool> CanEdit(int entityId, Profile profile)
        {
            if (profile.IsAdmin)
                return true;

            return await DbContext.Workers
                .Where(p => p.TopologyId == entityId
                    && p.PersonId == profile.Id
                    && p.Permission.HasFlag(Permission.Editor)
                    && !p.Topology.IsLocked)
                .AnyAsync();

        }
        public override async Task<bool> CanManage(int entityId, Profile profile)
        {
            if (profile.IsAdmin)
                return true;

            return await DbContext.Workers
                .Where(p => p.TopologyId == entityId
                    && p.PersonId == profile.Id
                    && p.Permission.HasFlag(Permission.Manager))
                .AnyAsync();

        }

        public override async Task Remove(Topology topology)
        {
            DbContext.Templates.RemoveRange(topology.Templates);
            DbContext.Remove(topology);
            await DbContext.SaveChangesAsync();
        }
    }
}