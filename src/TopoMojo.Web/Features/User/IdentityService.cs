// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using AutoMapper;
using TopoMojo.Data.Abstractions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class IdentityService
    {
        public IdentityService
        (
            IMapper mapper,
            IUserStore userStore
        )
        {
            _userStore = userStore;
            Mapper = mapper;
        }

        IMapper Mapper { get; }
        private readonly IUserStore _userStore;

        public async Task<User> Load(string globalId)
        {
            var profile = await _userStore.Retrieve(globalId);

            return (profile != null)
                ? Mapper.Map<User>(profile)
                : null;
        }

        public async Task<User> Add(User profile)
        {
            var entity = await _userStore.Create(
                Mapper.Map<Data.User>(profile)
            );

            return Mapper.Map<User>(entity);
        }

        public async Task Update(User profile)
        {
            var entity = await _userStore.Retrieve(profile.Id);

            Mapper.Map(profile, entity);

            await _userStore.Update(entity);
        }

    }
}
