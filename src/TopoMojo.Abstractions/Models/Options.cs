namespace TopoMojo.Models
{
    public class PodConfiguration {
        public string Type { get; set; }
        public string Url { get; set;}  //accepts range expansion
        public string Host { get; set;}
        public string User { get; set; }
        public string Password { get; set; }
        public string PoolPath { get; set; }
        public string Uplink { get; set; }
        public string VmStore { get; set; }
        public string DiskStore { get; set; }
        public string IsoStore { get; set; }
        public string StockStore { get; set; }
        public string DisplayMethod { get; set; }
        public string DisplayUrl { get; set; }
        public string TicketUrlHandler { get; set; } //"local-app", "external-domain", "host-map", "none"
        public VlanOptions Vlan { get; set; }
    }

    public class TemplateOptions {
        public string[] Cpu { get; set; }
        public string[] Ram { get; set; }
        public string[] Adapters { get; set; }
        public string[] Guest { get; set; }
        public string[] Iso { get; set; }
        public string[] Source { get; set; }
        public string[] Palette { get; set; }
    }

    public class VmOptions {
        public string[] Iso { get; set; }
        public string[] Net { get; set; }
    }

    public class VlanOptions
    {
        public string Range { get; set; }
        public Vlan[] Reservations { get; set; }
    }

    public class Vlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TaskStatus
    {
        public string Id { get; set; }
        public int Progress { get; set; }
    }

    public class KeyValuePair{
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}