using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
using TopoMojo.Data;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;

namespace TopoMojo.Core
{
    public class ProfileManager : EntityManager<Data.Entities.Profile>
    {
        public ProfileManager
        (
            IProfileRepository profileRepo,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        ) : base(profileRepo, mill, options, profileResolver)
        {
        }

        public async Task<Models.Profile> Add(Models.Profile profile)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            Data.Entities.Profile entity = await _profileRepo.Add(Mapper.Map<Data.Entities.Profile>(profile));
            return Mapper.Map<Models.Profile>(entity);
        }

        public async Task<Models.Profile> FindByGlobalId(string globalId)
        {
            Data.Entities.Profile profile = await _profileRepo.FindByGlobalId(globalId);
            return (profile != null)
                ? Mapper.Map<Models.Profile>(profile)
                : null;
        }


        // public async Task<bool> CanAccessGamespace(string guid)
        // {
        //     Player member = await _db.Players
        //         .Where(m => m.PersonId == _user.Id
        //             && m.Gamespace.GlobalId == guid)
        //         .SingleOrDefaultAsync();
        //     return (member != null);
        // }

        // public async Task<bool> CanEditWorkspace(string guid)
        // {
        //     if (_user.IsAdmin)
        //         return true;

        //     Worker permission = await _db.Workers
        //         .Where(p => p.PersonId == _user.Id
        //             && p.Topology.GlobalId == guid
        //             && p.Permission.HasFlag(Permission.Editor))
        //             .SingleOrDefaultAsync();
        //     return (permission != null);
        // }

    }
}