// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using TopoMojo.Data.Abstractions;
using TopoMojo.Models;

namespace TopoMojo.Data
{
    public class User: IEntity
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
        public int WorkspaceLimit { get; set; }
        public UserRole Role { get; set; }
        public virtual ICollection<Worker> Workspaces { get; set; } = new List<Worker>();
        public virtual ICollection<Player> Gamespaces { get; set; } = new List<Player>();
    }

}
