using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core
{
    public class Template : BaseModel
    {
        public int OwnerId { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public string Detail { get; set; }
    }

    public class TemplateReference
    {
        public int Id { get; set; }
        public int TopologyId { get; set; }
        public virtual Topology Topology { get; set; }
        public int TemplateId { get; set; }
        public virtual Template Template { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Iso { get; set; }
        public string Networks { get; set; }

        [NotMapped]
        public bool Owned { get { return Template != null && Template.OwnerId == TopologyId; } }
    }
}