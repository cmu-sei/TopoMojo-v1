namespace TopoMojo.Client
{
    public class Options
    {
        public string Url { get; set; }
        public string Key { get; set; }
        public int MaxRetries { get; set; } = 2;
    }
}
