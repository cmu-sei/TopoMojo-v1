// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopoMojo.Core.Models
{
    public class Gamespace
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string WhenCreated { get; set; }
        public string TopologyDocument { get; set; }
        public int TopologyId { get; set; }
        public Player[] Players { get; set; }
        //public Models.Vm[] Vms { get; set; }
        //public int VmCount { get; set; }
    }

    public class GameState
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GlobalId { get; set; }
        public string WhenCreated { get; set; }
        public string TopologyDocument { get; set; }
        public string ShareCode { get; set; }
        public Player[] Players { get; set; }
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
