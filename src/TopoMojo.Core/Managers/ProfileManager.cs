using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Data;
using TopoMojo.Core.Entities;

namespace TopoMojo.Core
{
    public class ProfileManager : EntityManager<Profile>
    {
        public ProfileManager
        (
            TopoMojoDbContext db,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        ) : base (db, mill, options, profileResolver)
        {
        }

        public async Task<Profile> LoadByGlobalId(string globalId)
        {
            return await _db.Profiles
                .Where(p => p.GlobalId == globalId)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> CanAccessGamespace(string guid)
        {
            Player member = await _db.Players
                .Where(m => m.PersonId == _user.Id
                    && m.Gamespace.GlobalId == guid)
                .SingleOrDefaultAsync();
            return (member != null);
        }

        public async Task<bool> CanEditWorkspace(string guid)
        {
            if (_user.IsAdmin)
                return true;

            Worker permission = await _db.Workers
                .Where(p => p.PersonId == _user.Id
                    && p.Topology.GlobalId == guid
                    && p.Permission.HasFlag(Permission.Editor))
                    .SingleOrDefaultAsync();
            return (permission != null);
        }
    }
}