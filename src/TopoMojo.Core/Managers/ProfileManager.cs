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

        public async Task<bool> CanEditSpace(string globalId)
        {
            return await _profileRepo.CanEditSpace(globalId, Profile);
        }

        public async Task<SearchResult<Models.Profile>> List(Search search)
        {
            IQueryable<Data.Entities.Profile> q = _profileRepo.List();
            if (search.Term.HasValue())
                q = q.Where(p => p.Name.Contains(search.Term));

            if (search.HasFilter("admins"))
                q = q.Where(p => p.IsAdmin);


            SearchResult<Models.Profile> result = new SearchResult<Models.Profile>();
            result.Search = search;
            result.Total = await q.CountAsync();

            if (search.Skip > 0)
                q = q.Skip(search.Skip);
            if (search.Take > 0)
                q = q.Take(search.Take);
            var list = await q.ToArrayAsync();
            result.Results = Mapper.Map<Models.Profile[]>(list);
            return result;
        }



    }
}