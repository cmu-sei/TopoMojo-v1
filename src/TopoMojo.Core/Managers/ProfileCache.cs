using System;
using System.Collections.Generic;
using System.Linq;
using TopoMojo.Data.Entities;

namespace TopoMojo.Core
{
    public interface IProfileCache
    {
        void Add(Profile profile);
        void Remove(string id);
        Profile Find(string id);
    }

    public class ProfileCache : IProfileCache
    {
        Dictionary<string, CachedProfile> _cache = new Dictionary<string, CachedProfile>();
        DateTime _lastPurge = DateTime.UtcNow;

        public void Add(Profile profile)
        {
            _cache.Add(
                profile.GlobalId,
                new CachedProfile {
                    Profile = profile,
                    Timestamp = DateTime.UtcNow
                }
            );
        }

        public void Remove(string globalId)
        {
            if (_cache.ContainsKey(globalId))
                _cache.Remove(globalId);
        }

        public Profile Find(string globalId)
        {
            if (_lastPurge.AddMinutes(10).CompareTo(DateTime.UtcNow)<0)
            {
                Purge();
            }

            return (_cache.ContainsKey(globalId))
                ? _cache[globalId].Profile
                : null;
        }

        public void Purge()
        {
            lock (_cache)
            {
                _lastPurge = DateTime.UtcNow;
                foreach (var cp in _cache.Values.ToArray())
                    if (cp.Timestamp.AddHours(1).CompareTo(_lastPurge) < 0)
                        _cache.Remove(cp.Profile.GlobalId);
            }
        }

        internal class CachedProfile
        {
            public Profile Profile { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}