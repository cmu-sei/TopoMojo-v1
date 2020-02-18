// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public class UserService : EntityService<Data.Profile>
    {
        public UserService
        (
            IUserStore userStore,
            IMemoryCache userCache,
            ILoggerFactory mill,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base(mill, mapper, options, identityResolver)
        {
            _userStore = userStore;
            _userCache = userCache;
        }

        private readonly IUserStore _userStore;
        private readonly IMemoryCache _userCache;

        public async Task<User> Add(User user)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            Data.Profile entity = await _userStore.Add(
                Mapper.Map<Data.Profile>(user)
            );

            return Mapper.Map<User>(entity);
        }

        public async Task<bool> PrivilegedUpdate(User profile)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            var entity = await _userStore.Load(profile.Id);

            Mapper.Map(profile, entity);

            entity.Role = entity.IsAdmin
                ? Data.ProfileRole.Administrator
                : Data.ProfileRole.User;

            await _userStore.Update(entity);

            _userCache.Remove(profile.GlobalId);
            return true;
        }

        public async Task<User> FindByGlobalId(string globalId)
        {
            Data.Profile profile = (globalId.HasValue())
                ? await _userStore.FindByGlobalId(globalId)
                : this.User;
            return (profile != null)
                ? Mapper.Map<User>(profile)
                : null;
        }

        public async Task<bool> CanEditSpace(string globalId)
        {
            return await _userStore.CanEditSpace(globalId, User);
        }

        public async Task<SearchResult<User>> List(Search search)
        {
            IQueryable<Data.Profile> q = _userStore.List();
            if (search.Term.HasValue())
                q = q.Where(p => p.Name.ToLower().Contains(search.Term.ToLower()));

            if (search.HasFilter("admins"))
                q = q.Where(p => p.IsAdmin);


            SearchResult<User> result = new SearchResult<User>();
            result.Search = search;
            result.Total = await q.CountAsync();

            q = q.OrderBy(p => p.Name);
            if (search.Skip > 0)
                q = q.Skip(search.Skip);
            if (search.Take > 0)
                q = q.Take(search.Take);
            var list = await q.ToArrayAsync();
            result.Results = Mapper.Map<User[]>(list);
            return result;
        }

        public async Task<bool> UpdateProfile(ChangedUser profile)
        {
            if (!User.IsAdmin && User.GlobalId != profile.GlobalId)
                throw new InvalidOperationException();

            var p = await _userStore.FindByGlobalId(profile.GlobalId);
            Mapper.Map(profile, p);
            await _userStore.Update(p);
            _userCache.Remove(profile.GlobalId);
            return true;
        }

        public async Task DeleteProfile(int id)
        {
            var entity = await _userStore.Load(id);

            if (entity == null || (!User.IsAdmin && User.GlobalId != entity.GlobalId))
                throw new InvalidOperationException();

            await _userStore.Remove(entity);

            _userCache.Remove(entity.GlobalId);
        }

    }
}
