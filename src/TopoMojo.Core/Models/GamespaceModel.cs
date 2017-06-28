using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core
{
    public class GamespaceSummary
    {
        public int Id { get; set; }
        public string WhenCreated { get; set; }
        public string Document { get; set; }
        public Models.Vm[] Vms { get; set; }
        public int VmCount { get; set; }
    }
}