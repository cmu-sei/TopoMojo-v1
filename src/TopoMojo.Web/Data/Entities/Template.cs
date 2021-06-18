// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class Template : IEntity
    {
        // public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public System.DateTime WhenCreated { get; set; }
        public string Description { get; set; }
        public string Iso { get; set; }
        public string Networks { get; set; }
        public string Guestinfo { get; set; }
        public bool IsHidden { get; set; }
        public bool IsPublished { get; set; }
        public string Detail { get; set; }
        public string ParentGlobalId { get; set; }
        public int Replicas { get; set; }
        public virtual Template Parent { get; set; }
        public string WorkspaceGlobalId { get; set; }
        public virtual Workspace Workspace { get; set; }
        public virtual ICollection<Template> Children { get; set; } = new List<Template>();
        [NotMapped] public bool IsLinked => string.IsNullOrEmpty(ParentGlobalId).Equals(false);
    }
}
