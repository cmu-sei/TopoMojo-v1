namespace TopoMojo.Hypervisor.vSphere
{
    public class VmContext
    {
        public Vm Vm { get; set; }
        public VimClient Host { get; set; }
    }
}
