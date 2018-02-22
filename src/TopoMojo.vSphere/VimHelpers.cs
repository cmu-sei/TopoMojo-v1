using System;

namespace TopoMojo.vSphere.Helpers
{
    public class PortGroupAllocation
    {
        public string Net { get; set; }
        public string Key { get; set; }
        public int Counter { get; set; }
        public int VlanId { get; set; }
    }

    internal class VimHostTask
    {
        public ManagedObjectReference Task { get; set; }
        public string Action { get; set; }
        public int Progress { get; set; }
        public DateTime WhenCreated { get; set; }
    }
}
