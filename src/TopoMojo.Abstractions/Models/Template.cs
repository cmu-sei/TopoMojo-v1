namespace TopoMojo.Models
{
    public class Template
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TopoId { get; set; }
        public string Cpu { get; set; }
        public string Guest { get; set; }
        public string Source { get; set; }
        public string Iso { get; set; }
        public string Floppy { get; set; }
        public KeyValuePair[] GuestSettings { get; set; }
        public string Version { get; set; }
        public string IsolationTag { get; set; }
        public int Ram { get; set; }
        public int VideoRam { get; set; }
        public int Adapters { get; set; }
        public int Delay { get; set; }
        public Eth[] Eth { get; set; }
        public Disk[] Disks { get; set; }
    }

    public class Eth
    {
        public int Id { get; set; }
        public string Net { get; set; }
        public string Type { get; set; }
        public string Mac { get; set; }
        public string Ip { get; set; }
        public int Vlan { get; set; }
    }

    public class Disk
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Source { get; set; }
        public string Controller { get; set; }
        public int Size { get; set; }
        public int Status { get; set; }
    }

}