namespace TopoMojo.Core
{
    public class CoreOptions
    {
        public int ConcurrentInstanceMaximum { get; set; } = 2;
        public int DefaultWorkspaceLimit { get; set; } = 0;
        public int WorkspaceTemplateLimit { get; set; } = 3;
    }
}