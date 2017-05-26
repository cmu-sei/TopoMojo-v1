using System.Collections.Generic;

namespace TopoMojo.Core
{
    public class Topology : BaseModel
    {
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public virtual ICollection<TemplateReference> Templates { get; set; } = new List<TemplateReference>();

    }

    public class TopoSummary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string People { get; set; }
        public string Templates { get; set; }
        public bool CanManage { get; set; }
    }
}