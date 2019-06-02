using System;
using System.Collections.Generic;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.Entities
{
    public class Gamespace : IEntityPrimary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
        public string ShareCode { get; set; }
        public int TopologyId { get; set; }
        public virtual Topology Topology { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }

}
