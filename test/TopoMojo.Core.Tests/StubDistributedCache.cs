// Copyright 2020 Carnegie Mellon University. 
// Released under a MIT (SEI) license. See LICENSE.md in the project root. 

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Tests
{
    public class StubDistributedCache : IDistributedCache
    {
        Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

        public byte[] Get(string key)
        {
            return _cache.ContainsKey(key)
                ? _cache[key]
                : null;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public void Refresh(string key)
        {

        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(0);
        }

        public void Remove(string key)
        {
            if (_cache.ContainsKey(key))
                _cache.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.FromResult(0);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (_cache.ContainsKey(key))
                _cache[key] = value;
            else
                _cache.Add(key, value);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.FromResult(0);
        }
    }
}
