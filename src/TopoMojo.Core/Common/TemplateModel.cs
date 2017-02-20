namespace TopoMojo.Core
{
    public class TemplateModel
    {
        public int Id { get; set; }
        public bool CanEdit { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
    }
}