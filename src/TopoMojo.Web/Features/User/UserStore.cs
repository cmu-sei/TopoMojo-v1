// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Data
{
    public class UserStore : CachedStore<User>, IUserStore
    {
        public UserStore (
            TopoMojoDbContext db,
            IMemoryCache memoryCache,
            IDistributedCache cache
        ) : base(db, memoryCache, cache)
        {

        }

        public async Task<User> LoadDetail(int id)
        {
            return await DbContext.Users
                .Include(p => p.Workspaces)
                .Include(p => p.Gamespaces)
                .Where(p => p.Id == id)
                .FirstAsync();
        }

        public override async Task<User> Create(User user)
        {
            string name = user.Name.ExtractBefore("@");

            if (!name.NotEmpty())
                user.Name = "Anonymous";

            if (!user.GlobalId.NotEmpty())
                user.GlobalId = Guid.NewGuid().ToString();

            user.WhenCreated = DateTime.UtcNow;

            if (!(await DbContext.Users.AnyAsync()))
            {
                // user.IsAdmin = true;
                user.Role = UserRole.Administrator;
            }

           return await base.Create(user);
        }

        // public async Task<bool> IsMember(string globalId, string userId)
        // {
        //     var user = await DbContext.Users
        //         .Where(u => u.GlobalId == userId)
        //         .FirstOrDefaultAsync();

        //     if (user?.Role == UserRole.Administrator)
        //         return true;

        //     if (await DbContext.Workspaces
        //         .Where(w => w.GlobalId == globalId && w.Workers.Any(t => t.Person.GlobalId == userId))
        //         .AnyAsync())
        //         return true;

        //     if (await DbContext.Gamespaces
        //         .Where(w => w.GlobalId == globalId && w.Players.Any(t => t.SubjectId == userId))
        //         .AnyAsync())
        //         return true;

        //     return false;
        // }

        public async Task<bool> CanInteract(string isolationId, string userId)
        {
            if (isolationId.IsEmpty() || userId.IsEmpty())
                return false;

            bool found = await DbContext.Players.AnyAsync(w =>
                w.Gamespace.GlobalId == isolationId &&
                w.SubjectId == userId
            );

            if (found.Equals(false))
                found = await DbContext.Workers.AnyAsync(w =>
                    w.Workspace.GlobalId == isolationId &&
                    w.Person.GlobalId == userId
                );

            return found;
        }

        // public async Task<bool> MemberOf(string globalId, Models.User user)
        // {
        //     if (user == null || string.IsNullOrEmpty(globalId))
        //         return false;

        //     if (user.IsAdmin)
        //         return true;

        //     bool result = false;

        //     var workspace = await DbContext.Workspaces
        //         .Include(t => t.Workers)
        //         .Where(t => t.GlobalId == globalId)
        //         .SingleOrDefaultAsync();

        //     result = workspace != null && workspace.CanEdit(user);

        //     if (result)
        //         workspace.LastActivity = DateTime.UtcNow;

        //     if (!result)
        //     {
        //         var gamespace = await DbContext.Gamespaces
        //             .Include(g => g.Players)
        //             .Where(t => t.GlobalId == globalId)
        //             .SingleOrDefaultAsync();

        //         result = gamespace != null & gamespace.CanEdit(user);
        //     }

        //     await DbContext.SaveChangesAsync();

        //     return result;
        // }

    }
}
