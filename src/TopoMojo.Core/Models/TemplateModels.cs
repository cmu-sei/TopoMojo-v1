namespace TopoMojo.Core.Models
{
    public class Template
    {
        public int Id { get; set; }
        public bool CanEdit { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public Template Parent { get; set; }
    }

    public class TemplateDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }
        public bool IsPublished { get; set; }
        public TemplateDetail Parent { get; set; }
    }

    public class LinkedTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TopologyId { get; set; }
        public string TopologyName { get; set; }
    }

}