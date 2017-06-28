using System.Collections.Generic;

namespace TopoMojo.Core
{
    public class TopoSummary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string People { get; set; }
        public string Templates { get; set; }
        public bool CanManage { get; set; }
        public bool IsPublished { get; set; }
    }
}