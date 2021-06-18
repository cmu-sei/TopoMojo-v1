using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;
using System;
using System.Collections.Generic;

namespace TopoMojo.Data
{
    public class Store<TEntity> : IStore<TEntity>
        where TEntity : class, IEntity
    {
        public Store(
            TopoMojoDbContext dbContext
        )
        {
            DbContext = dbContext;
        }

        public TopoMojoDbContext DbContext { get; set; }

        public virtual IQueryable<TEntity> List(string term = null)
        {
            return DbContext.Set<TEntity>();
        }

        public virtual async Task<TEntity> Create(TEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.GlobalId))
                entity.GlobalId = Guid.NewGuid().ToString("n");

            DbContext.Add(entity);

            await DbContext.SaveChangesAsync();

            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> range)
        {
            foreach (var entity in range)
                if (string.IsNullOrWhiteSpace(entity.GlobalId))
                    entity.GlobalId = Guid.NewGuid().ToString("n");

            DbContext.AddRange(range);

            await DbContext.SaveChangesAsync();

            return range;
        }

        public virtual async Task<TEntity> Retrieve(int id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
        {
            if (includes == null)
                return await DbContext.Set<TEntity>().FindAsync(id);

            return await includes.Invoke(DbContext.Set<TEntity>())
                .Where(e => e.Id == id)
                .SingleOrDefaultAsync();

        }

        public virtual async Task<TEntity> Retrieve(string id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
        {
            if (includes == null)
                return await DbContext.Set<TEntity>()
                    .Where(e => e.GlobalId == id)
                    .SingleOrDefaultAsync();
            // TODO: change this after transitioning to string PK
            //     return await DbContext.Set<TEntity>().FindAsync(id);

            return await includes.Invoke(DbContext.Set<TEntity>())
                .Where(e => e.GlobalId == id)
                .SingleOrDefaultAsync();
        }

        public virtual async Task Update(TEntity entity)
        {
            DbContext.Update(entity);

            await DbContext.SaveChangesAsync();
        }

        public virtual async Task Update(IEnumerable<TEntity> range)
        {
            DbContext.UpdateRange(range);

            await DbContext.SaveChangesAsync();
        }

        public virtual async Task Delete(string id)
        {
            var entity = await Retrieve(id);

            if (entity is TEntity)
            {
                DbContext.Set<TEntity>().Remove(entity);

                await DbContext.SaveChangesAsync();
            }
        }

    }
}
