using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core
{
    public class Instance : BaseModel
    {
        public int TopologyId { get; set; }
        public virtual Topology Topology { get; set; }
        public virtual ICollection<InstanceMember> Members { get; set; }
    }

    public class InstanceMember
    {
        public int Id { get; set; }
        public int InstanceId { get; set; }
        public virtual Instance Instance { get; set; }
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
        public bool isAdmin { get; set; }
    }

    public class InstanceSummary
    {
        public int Id { get; set; }
        public string WhenCreated { get; set; }
        public string Document { get; set; }
        public Models.Vm[] Vms { get; set; }
        public int VmCount { get; set; }
    }
}