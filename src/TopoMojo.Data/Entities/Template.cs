// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.Entities
{
    public class Template : IEntityPrimary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public System.DateTime WhenCreated { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Iso { get; set; } = string.Empty;
        public string Networks { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public bool IsPublished { get; set; }
        public string Detail { get; set; }
        public int? ParentId { get; set; }
        public virtual Template Parent { get; set; }
        public int? TopologyId { get; set; }
        public virtual Topology Topology { get; set; }

    }
}
