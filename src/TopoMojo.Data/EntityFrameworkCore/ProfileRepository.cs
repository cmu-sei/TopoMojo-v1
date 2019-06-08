// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

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
            if (!(await DbContext.Profiles.AnyAsync()))
            {
                profile.IsAdmin = true;
                profile.Role = ProfileRole.Administrator;
            }
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
                    .Include(w => w.Topology)
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

                if (gamespace != null)
                {
                    result = await DbContext.Players
                    .Where(p => p.GamespaceId == gamespace.Id
                        && p.PersonId == profile.Id
                        && p.Permission.HasFlag(Permission.Editor))
                    .AnyAsync();
                }
            }

            return result;
        }

        public async Task<bool> IsEmpty()
        {
            return !(await DbContext.Profiles.AnyAsync());
        }
    }
}
