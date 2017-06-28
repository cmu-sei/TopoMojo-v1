using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core.Entities
{
    public class Template : Entity
    {
        public int OwnerId { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public string Detail { get; set; }
        public virtual ICollection<Linker> Linkers {get; set; } = new List<Linker>();
    }
}