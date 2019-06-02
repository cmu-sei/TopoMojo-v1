// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

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