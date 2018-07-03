using System;
using System.Collections.Concurrent;

namespace TopoMojo.Services
{
    public class HubCache
    {
        public ConcurrentDictionary<string,CachedConnection> Connections { get; } = new ConcurrentDictionary<string,CachedConnection>();
    }

    public class CachedConnection
    {
        public string Id { get; set; }
        public string Room { get; set; }
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
    }

}