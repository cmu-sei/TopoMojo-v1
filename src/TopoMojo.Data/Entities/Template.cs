namespace TopoMojo.Data.Entities
{
    public class Template : Data.Abstractions.IEntity
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public System.DateTime WhenCreated { get; set; }
        public string Description { get; set; }
        public string Iso { get; set; }
        public string Networks { get; set; }
        public bool IsHidden { get; set; }
        public bool IsPublished { get; set; }
        public string Detail { get; set; }
        public int? ParentId { get; set; }
        public virtual Template Parent { get; set; }
        public int? TopologyId { get; set; }
        public virtual Topology Topology { get; set; }

    }
}