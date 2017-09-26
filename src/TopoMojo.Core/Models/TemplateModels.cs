namespace TopoMojo.Core.Models
{
    public class Template
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public bool CanEdit { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public bool IsHidden { get; set; }
        public int TopologyId { get; set; }
        public string TopologyGlobalId { get; set; }
        //  public TemplateSummary Parent { get; set; }
        // public string ParentId { get; set; }
        // public string ParentName { get; set; }
    }

    public class ChangedTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public bool IsHidden { get; set; }
        public int TopologyId { get; set; }
    }

    public class NewTemplateDetail
    {
        public string Name { get; set; }
        public string Networks { get; set; }
        public string Detail { get; set; }
        public bool IsPublished { get; set; }
    }

    public class TemplateDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Detail { get; set; }
        public bool IsPublished { get; set; }
    }

    public class TemplateSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TopologyId { get; set; }
        public string TopologyName { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }
    }

}