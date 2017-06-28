using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core.Entities
{
    public class Topology : Entity
    {
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
        public string ShareCode { get; set; }
        public bool IsPublished { get; set; }
        public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public virtual ICollection<Gamespace> Gamespaces { get; set; } = new List<Gamespace>();
        public virtual ICollection<Linker> Linkers { get; set; } = new List<Linker>();

        [NotMapped]
        public string Document
        {
            get {
                return (this.DocumentUrl.HasValue())
                        ? this.DocumentUrl
                        : "/docs/" + this.GlobalId + ".md";
            }
        }
    }
}