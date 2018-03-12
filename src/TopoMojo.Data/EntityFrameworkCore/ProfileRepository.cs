using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.EntityFrameworkCore
{
    public class ProfileRepository : Repository<Profile>, IProfileRepository
    {
        public ProfileRepository (
            TopoMojoDbContext db
        ) : base(db) { }

        public async Task<Profile> LoadDetail(int id)
        {
            return await DbContext.Profiles
                .Include(p => p.Workspaces)
                .Include(p => p.Gamespaces)
                .Where(p => p.Id == id)
                .FirstAsync();
        }

        public override async Task<Profile> Add(Profile profile)
        {
            string name = profile.Name.ExtractBefore("@");
            profile.Name = (name.HasValue()) ? name : "Anonymous";
            profile.GlobalId = (profile.GlobalId.HasValue()) ? profile.GlobalId : Guid.NewGuid().ToString();
            profile.WhenCreated = DateTime.UtcNow;
            DbContext.Profiles.Add(profile);
            await DbContext.SaveChangesAsync();
            return profile;
        }

        public async Task<bool> CanEditSpace(string globalId, Profile profile)
        {
            bool result = false;

            if (profile.IsAdmin)
                result = true;

            if (!result)
            {
                Topology topology = await DbContext.Topologies
                    .Where(t => t.GlobalId == globalId)
                    .SingleOrDefaultAsync();

                if (topology != null)
                {
                    result = await DbContext.Workers
                    .Where(p => p.TopologyId == topology.Id
                        && p.PersonId == profile.Id
                        && p.Permission.HasFlag(Permission.Editor)
                        && !p.Topology.IsLocked)
                    .AnyAsync();
                }
            }

            if (!result)
            {
                Gamespace gamespace = await DbContext.Gamespaces
                    .Where(t => t.GlobalId == globalId)
                    .SingleOrDefaultAsync();

                result = await DbContext.Players
                .Where(p => p.GamespaceId == gamespace.Id
                    && p.PersonId == profile.Id
                    && p.Permission.HasFlag(Permission.Editor))
                .AnyAsync();
            }

            return result;
        }
    }
}