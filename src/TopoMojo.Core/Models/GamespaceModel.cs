using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core.Models
{
    public class Gamespace
    {
        public int Id { get; set; }
        public string WhenCreated { get; set; }
        public string Document { get; set; }
        //public Models.Vm[] Vms { get; set; }
        public int VmCount { get; set; }
    }

    public class GameState
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string WhenCreated { get; set; }
        public string Document { get; set; }
        public string ShareCode { get; set; }
        public IEnumerable<VmState> Vms { get; set; } = new List<VmState>();

    }

    public class VmState
    {
        public string Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public bool IsRunning { get; set; }
    }

}