namespace TopoMojo.Core
{
    public class SimulationSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanEdit { get; set; }
        public bool IsPublished { get; set; }
        public string[] Authors { get; set; }
        public string WhenCreated { get; set; }

    }
}