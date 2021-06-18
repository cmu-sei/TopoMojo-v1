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
        // public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Scope { get; set; }
        public string CallbackUrl { get; set; }
        public int WorkspaceLimit { get; set; }
        public int GamespaceLimit { get; set; }
        public int SessionLimit { get; set; }
        public int GamespaceMaxMinutes { get; set; }
        public UserRole Role { get; set; }
        public DateTime WhenCreated { get; set; }
        public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
        // public virtual ICollection<Worker> Workspaces { get; set; } = new List<Worker>();
        // public virtual ICollection<Player> Gamespaces { get; set; } = new List<Player>();
    }

    public class ApiKey
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
