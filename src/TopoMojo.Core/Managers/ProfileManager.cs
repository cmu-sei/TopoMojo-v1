using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models.Extensions;
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
            IProfileResolver profileResolver,
            IProfileCache profileCache
        ) : base(profileRepo, mill, options, profileResolver, profileCache)
        {
        }

        public async Task<Models.Profile> Add(Models.Profile profile)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            Data.Entities.Profile entity = await _profileRepo.Add(Mapper.Map<Data.Entities.Profile>(profile));
            return Mapper.Map<Models.Profile>(entity);
        }

        public async Task<bool> PrivilegedUpdate(Models.Profile profile)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            var entity = await _profileRepo.Load(profile.Id);
            Mapper.Map(profile, entity);
            await _profileRepo.Update(entity);
            _profileCache.Remove(entity.GlobalId);
            return true;
        }

        public async Task<Models.Profile> FindByGlobalId(string globalId)
        {
            Data.Entities.Profile profile = await _profileRepo.FindByGlobalId(globalId);
            return (profile != null)
                ? Mapper.Map<Models.Profile>(profile)
                : null;
        }

        public async Task<bool> CanEditSpace(string globalId)
        {
            return await _profileRepo.CanEditSpace(globalId, Profile);
        }

        public async Task<Models.SearchResult<Models.Profile>> List(Models.Search search)
        {
            IQueryable<Data.Entities.Profile> q = _profileRepo.List();
            if (search.Term.HasValue())
                q = q.Where(p => p.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase)>-1);

            if (search.HasFilter("admins"))
                q = q.Where(p => p.IsAdmin);


            Models.SearchResult<Models.Profile> result = new Models.SearchResult<Models.Profile>();
            result.Search = search;
            result.Total = await q.CountAsync();

            q = q.OrderBy(p => p.Name);
            if (search.Skip > 0)
                q = q.Skip(search.Skip);
            if (search.Take > 0)
                q = q.Take(search.Take);
            var list = await q.ToArrayAsync();
            result.Results = Mapper.Map<Models.Profile[]>(list);
            return result;
        }

        public async Task<bool> UpdateProfile(Models.ChangedProfile profile)
        {
            if (!Profile.IsAdmin || Profile.GlobalId != profile.GlobalId)
                throw new InvalidOperationException();

            var p = await _profileRepo.FindByGlobalId(profile.GlobalId);
            Mapper.Map(profile, p);
            await _profileRepo.Update(p);
            return true;
        }


    }
}