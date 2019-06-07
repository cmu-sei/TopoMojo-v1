// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models.Extensions;
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
            IMemoryCache profileCache
        ) : base(mill, options, profileResolver)
        {
            _profileRepo = profileRepo;
            _profileCache = profileCache;
        }

        private readonly IProfileRepository _profileRepo;
        private readonly IMemoryCache _profileCache;
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
            _profileCache.Remove(profile.GlobalId);
            return true;
        }

        public async Task<Models.Profile> FindByGlobalId(string globalId)
        {
            Data.Entities.Profile profile = (globalId.HasValue())
                ? await _profileRepo.FindByGlobalId(globalId)
                : this.Profile;
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
                q = q.Where(p => p.Name.IndexOf(search.Term, StringComparison.OrdinalIgnoreCase) >= 0);

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
            if (!Profile.IsAdmin && Profile.GlobalId != profile.GlobalId)
                throw new InvalidOperationException();

            var p = await _profileRepo.FindByGlobalId(profile.GlobalId);
            Mapper.Map(profile, p);
            await _profileRepo.Update(p);
            _profileCache.Remove(profile.GlobalId);
            return true;
        }

        public async Task DeleteProfile(int id)
        {
            var entity = await _profileRepo.Load(id);

            if (entity == null || (!Profile.IsAdmin && Profile.GlobalId != entity.GlobalId))
                throw new InvalidOperationException();

            await _profileRepo.Remove(entity);
        }

    }
}
