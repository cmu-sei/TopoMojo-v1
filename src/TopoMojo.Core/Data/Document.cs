using System;
using System.Collections.Generic;

namespace TopoMojo.Core
{
    public class Document : BaseModel
    {
        public int SimulationId { get; set; }
        public virtual Simulation Simulation { get; set; }
        public virtual ICollection<DocumentSection> Sections { get; set; } = new List<DocumentSection>();
    }

    public class DocumentSection
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }
        public int Order { get; set; }
    }
}