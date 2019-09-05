namespace TopoMojo.Core.Models
{
    public class NewGamespace
    {
        public string Id { get; set; }
        public WorkspaceSpec Workspace { get; set; }
    }

    public class WorkspaceSpec
    {
        public int Id { get; set; }
        public NetworkSpec Network { get; set; }
        public VmSpec[] Vms { get; set; }
        public bool CustomizeTemplates { get; set; }
        public string Templates { get; set; }
    }

    public class VmSpec
    {
        public string Name { get; set; }
        public int Replicas { get; set; }
    }

    public class NetworkSpec
    {
        public string[] Dns { get; set; }
        public string DhcpIp { get; set; }
        public string DhcpRange { get; set; }
    }
}
