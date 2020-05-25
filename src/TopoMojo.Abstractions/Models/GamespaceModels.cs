// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Collections.Generic;

namespace TopoMojo.Models
{
    public class Gamespace
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string WhenCreated { get; set; }
        public string TopologyDocument { get; set; }
        public int TopologyId { get; set; }
        public Player[] Players { get; set; }
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

    public class Player
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string PersonGlobalId { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
    }
}
