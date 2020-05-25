// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public abstract class CachedStore<TEntity> : DataStore<TEntity>
        where TEntity : class, IEntity
    {
        public CachedStore(
            TopoMojoDbContext dbContext,
            IMemoryCache memoryCache,
            IDistributedCache cache = null
        ) : base(dbContext, memoryCache)
        {
            _cache = cache;
        }

        protected IDistributedCache _cache { get; }

        protected JsonSerializerSettings _serializeSettings => new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        protected DistributedCacheEntryOptions _cacheOptions => new DistributedCacheEntryOptions
        {
            SlidingExpiration = new TimeSpan(0, 15, 0)
        };

        protected override async Task<object> FromCache(string key)
        {
            if (_cache == null)
                return null;

            TEntity entity = null;

            try
            {

                string v = await _cache.GetStringAsync(key);

                if (!string.IsNullOrEmpty(v))
                    entity = JsonConvert.DeserializeObject<TEntity>(v);

            }
            catch
            {
            }

            return entity;
        }

        protected override async Task ToCache(string key, TEntity value, DistributedCacheEntryOptions options = null)
        {
            if (_cache == null)
                return;

            try
            {
                string v = JsonConvert.SerializeObject(value, _serializeSettings);

                await _cache.SetStringAsync(key, v, options ?? _cacheOptions);
            }

            catch {}
        }

        protected override async Task UnCache(string key)
        {
            if (_cache != null)
                await _cache.RemoveAsync(key);
        }

    }
}
