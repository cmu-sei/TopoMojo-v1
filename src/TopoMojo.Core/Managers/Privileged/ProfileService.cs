// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

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

namespace TopoMojo.Core.Privileged
{
    public class ProfileService
    {
        public ProfileService
        (
            IProfileRepository profileRepo
        )
        {
            _repo = profileRepo;
        }

        private readonly IProfileRepository _repo;
        public async Task<Models.Profile> Add(Models.Profile profile)
        {
            Data.Entities.Profile entity = await _repo.Add(Mapper.Map<Data.Entities.Profile>(profile));
            return Mapper.Map<Models.Profile>(entity);
        }

        public async Task Update(Models.Profile profile)
        {
            var entity = await _repo.Load(profile.Id);
            Mapper.Map(profile, entity);
            await _repo.Update(entity);
        }

        public async Task<Models.Profile> FindByGlobalId(string globalId)
        {
            Data.Entities.Profile profile = await _repo.FindByGlobalId(globalId);
            return (profile != null)
                ? Mapper.Map<Models.Profile>(profile)
                : null;
        }
    }
}
