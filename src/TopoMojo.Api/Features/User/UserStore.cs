// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
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
    public class UserStore : Store<User>, IUserStore
    {
        public UserStore (
            TopoMojoDbContext db
        ) : base(db)
        {

        }

        public override async Task<User> Create(User user)
        {
            string name = user.Name.ExtractBefore("@");

            if (name.IsEmpty())
                user.Name = "Anonymous";

            if (user.Id.IsEmpty())
                user.Id = Guid.NewGuid().ToString();

            user.WhenCreated = DateTime.UtcNow;

            if (!(await DbSet.AnyAsync()))
                user.Role = UserRole.Administrator;

            return await base.Create(user);
        }

        public async Task<bool> CanInteract(string isolationId, string userId)
        {
            if (isolationId.IsEmpty() || userId.IsEmpty())
                return false;

            bool found = await DbContext.Players.AnyAsync(w =>
                w.SubjectId == userId &&
                w.GamespaceId == isolationId
            );

            if (found.Equals(false))
                found = await DbContext.Workers.AnyAsync(w =>
                    w.SubjectId == userId &&
                    w.WorkspaceId == isolationId
                );

            return found;
        }

        public Task<User> LoadWithKeys(string id)
        {
            return DbSet
                .Include(u => u.ApiKeys)
                .FirstOrDefaultAsync(
                    u => u.Id == id
                )
            ;
        }

        public async Task DeleteApiKey(string id)
        {
            var entity = await DbContext.ApiKeys.FindAsync(id);

            if (entity == null)
                return;

            DbContext.ApiKeys.Remove(entity);

            await DbContext.SaveChangesAsync();
        }

        public async Task<User> ResolveApiKey(string hash)
        {
            return await DbSet.FirstOrDefaultAsync(u =>
                u.ApiKeys.Any(k => k.Hash == hash)
            );
        }
    }
}
