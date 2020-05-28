// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;

namespace TopoMojo.Data
{
    public abstract class Store<TEntity> : IStore<TEntity>
        where TEntity : class, IEntity
    {
        protected TopoMojoDbContext DbContext { get; }

        public Store(TopoMojoDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            // entity.Name = entity.Name ?? $"[{typeof(TEntity).Name}Name]";
            entity.WhenCreated = DateTime.UtcNow;

            if (!entity.GlobalId.HasValue() || !Guid.TryParse(entity.GlobalId, out Guid guid))
                entity.GlobalId = Guid.NewGuid().ToString();

            DbContext.Add(entity);

            await DbContext.SaveChangesAsync();

            return entity;
        }

        public virtual async Task Update(TEntity entity)
        {
            // entity.Name = entity.Name ?? $"[{typeof(TEntity).Name}Name]";

            DbContext.Update(entity);

            await DbContext.SaveChangesAsync();
        }

        public virtual async Task Remove(TEntity entity)
        {
            DbContext.Set<TEntity>().Remove(entity);
            await DbContext.SaveChangesAsync();
        }

        public virtual async Task<TEntity> Load(int id)
        {
            return await DbContext.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<TEntity> FindByGlobalId(string guid)
        {
            int id = await DbContext.Set<TEntity>()
                .Where(e => e.GlobalId == guid)
                .Select(e => e.Id)
                .SingleOrDefaultAsync();

            return await Load(id);
        }

        public virtual IQueryable<TEntity> List()
        {
            return DbContext.Set<TEntity>();
        }

        public virtual async Task<bool> CanEdit(int id, User profile)
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> CanManage(int id, User profile)
        {
            return await Task.FromResult(false);
        }
    }

    public class RepositoryContext : IStoreUserContext
    {
        public int UserId { get; set; }
        public bool UserIsAdmin { get; set; }
    }
}
