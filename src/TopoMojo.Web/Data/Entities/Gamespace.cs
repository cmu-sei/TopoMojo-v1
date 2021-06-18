// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class Gamespace : IEntity
    {
        // public int Id { get; set; }
        public string GlobalId { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string ShareCode { get; set; }
        public DateTime WhenCreated { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool AllowReset { get; set; }
        public string Challenge { get; set; }
        public string WorkspaceGlobalId { get; set; }
        public virtual Workspace Workspace { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }

}
