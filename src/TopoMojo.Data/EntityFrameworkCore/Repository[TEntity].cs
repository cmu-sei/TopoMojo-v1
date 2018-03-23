using TopoMojo.Data.Abstractions;
using TopoMojo.Data;
using TopoMojo.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Data.EntityFrameworkCore
{
    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntityPrimary
    {
        protected TopoMojoDbContext DbContext { get; private set; }

        public Repository(TopoMojoDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            entity.Name = entity.Name ?? $"[{typeof(TEntity).Name}Name]";
            entity.WhenCreated = DateTime.UtcNow;
            if (!entity.GlobalId.HasValue() || !Guid.TryParse(entity.GlobalId, out Guid guid))
                entity.GlobalId = Guid.NewGuid().ToString();
            DbContext.Add(entity);
            await DbContext.SaveChangesAsync();
            return entity;
        }

        public virtual async Task Update(TEntity entity)
        {
            entity.Name = entity.Name ?? $"[{typeof(TEntity).Name}Name]";
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

        public virtual async Task<bool> CanEdit(int id, Profile profile)
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> CanManage(int id, Profile profile)
        {
            return await Task.FromResult(false);
        }
    }

    public class RepositoryContext : IRepositoryContext
    {
        public int UserId { get; set; }
        public bool UserIsAdmin { get; set; }
    }
}