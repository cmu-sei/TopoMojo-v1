// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
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
        public string Audience { get; set; }
        public int WorkspaceId { get; set; }
        public virtual Workspace Workspace { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }

}
