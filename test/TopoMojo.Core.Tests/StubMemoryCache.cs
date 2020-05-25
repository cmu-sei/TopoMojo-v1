using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace Tests
{
    public class StubMemoryCache : IMemoryCache
    {
        Dictionary<object, object> _cache = new Dictionary<object, object>();

        public ICacheEntry CreateEntry(object key)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Remove(object key)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(object key, out object value)
        {
            throw new System.NotImplementedException();
        }
    }
}
