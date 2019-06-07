// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.Entities
{
    public class Worker
    {
        public int Id { get; set; }
        public int TopologyId { get; set; }
        public virtual Topology Topology { get; set; }
        public int PersonId { get; set; }
        public virtual Profile Person { get; set; }
        public Permission Permission { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}
