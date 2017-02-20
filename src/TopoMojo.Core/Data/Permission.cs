using System;

namespace TopoMojo.Core
{
    public class Permission
    {
        public int Id { get; set; }
        public int TopologyId { get; set; }
        public virtual Topology Topology { get; set; }
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
        public PermissionFlag Value { get; set; }
    }

    [Flags]
    public enum PermissionFlag {
        None = 0,
        Editor = 0x01,
        Manager = 0xff
    }

    public enum EntityType
    {
        None,
        Simulation,
        Topology
    }
}