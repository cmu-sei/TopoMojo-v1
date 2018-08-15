using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.Entities
{
    public class Topology : IEntityPrimary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string ShareCode { get; set; }
        public string Author { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public bool IsLocked { get; set; }
        public int TemplateLimit { get; set; }
        public bool UseUplinkSwitch { get; set; }
        public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public virtual ICollection<Gamespace> Gamespaces { get; set; } = new List<Gamespace>();
        public virtual ICollection<Template> Templates { get; set; } = new List<Template>();

        [Obsolete]
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
