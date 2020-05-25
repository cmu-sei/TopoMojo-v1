// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class Gamespace : IEntity
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
        public DateTime LastActivity { get; set; }
        public string ShareCode { get; set; }
        public int TopologyId { get; set; }
        public virtual Topology Topology { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }

}
