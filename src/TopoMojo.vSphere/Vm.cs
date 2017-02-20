namespace TopoMojo.Conflicted
{
    public class Vm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Reference { get; set; }
        public string Stats { get; set; }
        public VmPowerState State { get; set; }
        public VmQuestion Question { get; set; }
    }

    public enum VmPowerState { off, running, suspended}

    public class VmQuestion
    {
        public string Id { get; set; }
        public string Prompt { get; set; }
        public int DefaultChoice { get; set; }
        public string[] Choices { get; set; }
        public string[] Labels { get; set; }
    }

    public class DisplayInfo
    {
        public string Id { get; set; }
        public string TopoId { get; set; }
        public string Name { get; set; }
        public DisplayMethod Method { get; set; }
        public string Url { get; set; }
        public string Conditions { get; set; }
    }

    public enum DisplayMethod { mock, wmks, guac, vmrc}
}