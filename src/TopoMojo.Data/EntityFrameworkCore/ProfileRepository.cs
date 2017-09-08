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

        public async Task<Profile> LoadOrCreate(Profile profile)
        {
            Profile result = null;

            if (profile.Id > 0)
                result = await Load(profile.Id);

            if (result == null)
                result = await LoadByGlobalId(profile.GlobalId);

            if (result == null)
                result = await Add(profile);

            return result;
        }

        public override async Task<Profile> Add(Profile profile)
        {
            profile.Name = (profile.Name.HasValue()) ? profile.Name : "Anonymous";
            profile.GlobalId = (profile.GlobalId.HasValue()) ? profile.GlobalId : Guid.NewGuid().ToString();
            profile.WhenCreated = DateTime.UtcNow;
            DbContext.Profiles.Add(profile);
            await DbContext.SaveChangesAsync();
            return profile;
        }

        public async Task<Profile> LoadByGlobalId(string guid)
        {
            return await DbContext.Profiles
                    .Where(p => p.GlobalId == guid)
                    .SingleOrDefaultAsync();

        }
    }
}