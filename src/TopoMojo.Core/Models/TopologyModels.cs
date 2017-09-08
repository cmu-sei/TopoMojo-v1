using System.Collections.Generic;

namespace TopoMojo.Core.Models
{
    public class Topology
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
        public string ShareCode { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        public bool IsPublished { get; set; }
        public Worker[] Workers { get; set; }
        public TopologyTemplate[] Templates { get; set; }
    }

    public class NewTopology
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ChangedTopology
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Worker
    {
        public int Id { get; set; }
        public string PersonName { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
    }

    public class TopologyTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
    }
}