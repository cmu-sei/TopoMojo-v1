// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public abstract class DataStore<TEntity> : IDataStore<TEntity>
        where TEntity : class, IEntity
    {
        public DataStore(
            TopoMojoDbContext dbContext,
            IMemoryCache idmap,
            IDistributedCache cache = null
        )
        {
            DbContext = dbContext;

            IdMap = idmap;
            // _cache = cache;

            // _serializeSettings = new JsonSerializerSettings
            // {
            //     ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            // };

            // _cacheOptions = new DistributedCacheEntryOptions
            // {
            //     SlidingExpiration = new TimeSpan(0, 15, 0)
            // };

        }

        public TopoMojoDbContext DbContext { get; }
        protected IMemoryCache IdMap { get; }
        // protected IDistributedCache _cache { get; }
        protected Dictionary<int, TEntity> ScopedCache => new Dictionary<int, TEntity>();
        // protected Dictionary<string, int> IdMap => new Dictionary<string, int>();
        // protected JsonSerializerSettings _serializeSettings => new JsonSerializerSettings
        // {
        //     ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        // };
        // protected DistributedCacheEntryOptions _cacheOptions => new DistributedCacheEntryOptions
        // {
        //     SlidingExpiration = new TimeSpan(0, 15, 0)
        // };

        public virtual IQueryable<TEntity> List(string term = null)
        {
            return DbContext.Set<TEntity>().AsNoTracking();
        }

        public virtual async Task<TEntity> Load(int id)
        {
            return await Load(id, null);
        }

        public virtual async Task<TEntity> Load(int id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes)
        {
            if (id == 0)
                return null;

            if (ScopedCache.ContainsKey(id))
                return ScopedCache[id];

            var entity = await FromCache(id);

            if (entity == null) {

                IQueryable<TEntity> query = DbContext.Set<TEntity>();

                if (includes != null)
                    query = includes(query);

                entity = await query.Where(e => e.Id == id).SingleOrDefaultAsync();

                await ToCache(entity);

            }
            else
            {

                DbContext.Set<TEntity>().Attach(entity);

            }

            ScopedCache.Add(id, entity);

            return entity;
        }

        public virtual async Task<TEntity> Load(string id)
        {
            return await Load(id, null);
        }

        public virtual async Task<TEntity> Load(string id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            if (IdMap.TryGetValue(id, out int i))
                return await Load(i, includes);

            int actualId = await DbContext.Set<TEntity>()
                .Where(e => e.GlobalId == id)
                .Select(e => e.Id)
                .SingleOrDefaultAsync();

            return await Load(actualId, includes);
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            entity.WhenCreated = DateTime.UtcNow;

            if (string.IsNullOrEmpty(entity.GlobalId))
                entity.GlobalId = Guid.NewGuid().ToString();

            DbContext.Add(entity);

            await DbContext.SaveChangesAsync();

            return await Load(entity.Id);
        }

        public virtual async Task Update(TEntity entity)
        {
            DbContext.Update(entity);

            await DbContext.SaveChangesAsync();

            await UnCache(entity);
        }

        public virtual async Task Delete(int id)
        {
            var entity = await Load(id);

            if (entity != null)
            {
                DbContext.Set<TEntity>().Remove(entity);

                await DbContext.SaveChangesAsync();

                await UnCache(entity);
            }

        }

        public async Task<bool> Exists(int id)
        {
            var entity = await Load(id);

            return entity is TEntity;
        }

        #region Entity Cache Helpers

        protected virtual async Task<TEntity> FromCache(int id)
        {
            string key = $"{typeof(TEntity).FullName}:{id}";

            return await FromCache(key) as TEntity;
        }

        protected virtual async Task ToCache(TEntity entity)
        {
            if (entity == null)
                return;

            string key = $"{entity.GetType().FullName}:{entity.Id}";

            await ToCache(key, entity);

            IdMap.Set(entity.GlobalId, entity.Id);
        }

        protected virtual async Task UnCache(TEntity entity)
        {
            if (entity == null)
                return;

            string key = $"{entity.GetType().FullName}:{entity.Id}";

            await UnCache(key);
        }

        #endregion

        #region Distributed Cache methods

        protected virtual async Task<object> FromCache(string key)
        {
            TEntity entity = null;

            // if (_cache == null)
            //     return null;

            // try
            // {

            //     string v = await _cache.GetStringAsync(key);

            //     if (!string.IsNullOrEmpty(v))
            //         entity = JsonConvert.DeserializeObject<TEntity>(v);

            // }
            // catch
            // {
            // }

            return entity;
        }

        protected virtual async Task ToCache(string key, TEntity value, DistributedCacheEntryOptions options = null)
        {
            // if (_cache == null)
            //     return;

            // try
            // {
            //     string v = JsonConvert.SerializeObject(value, _serializeSettings);

            //     await _cache.SetStringAsync(key, v, options ?? _cacheOptions);
            // }

            // catch {}
        }

        protected virtual async Task UnCache(string key)
        {
            // if (_cache != null)
            //     await _cache.RemoveAsync(key);
        }

        #endregion
    }
}
