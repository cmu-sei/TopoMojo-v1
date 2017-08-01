using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core.Entities
{
    public class Gamespace : Entity
    {
        public int TopologyId { get; set; }
        public string ShareCode { get; set; }
        public virtual Topology Topology { get; set; }
        public virtual ICollection<Player> Players { get; set; }
    }

}