using System.Collections.Generic;

namespace TopoMojo.Core.Models
{
    public class Topology
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Document { get; set; }
        public string ShareCode { get; set; }
        public string Author { get; set; }
        public string WhenCreated { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        public int TemplateLimit { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLocked { get; set; }
        public int GamespaceCount { get; set; }
        public Worker[] Workers { get; set; }
        public Template[] Templates { get; set; }
    }

    public class TopologySummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLocked { get; set; }
        public string Author { get; set; }
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
        public string Author { get; set; }
    }

    public class TopologyState
    {
        public int Id { get; set; }
        public string ShareCode { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLocked { get; set; }
    }

    public class Worker
    {
        public int Id { get; set; }
        public string PersonName { get; set; }
        public string PersonGlobalId { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
    }

    // public class TopologyTemplate
    // {
    //     public int Id { get; set; }
    //     public string Name { get; set; }
    //     public string ParentName { get; set; }
    // }
}
