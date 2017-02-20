
using System;
using System.Collections.Generic;

namespace TopoMojo.Core
{
    public class Simulation : BaseModel
    {
        public string Description { get; set; }
        public virtual Topology Topology { get; set; }
        public virtual Document Document { get; set; }
    }


}