namespace TopoMojo
{
    public class ControlOptions
    {
        public string ApplicationName { get; set; }
        public bool ShowExceptionDetail { get; set; }
        public int ProfileCacheSeconds { get; set; } = 300;
    }
}