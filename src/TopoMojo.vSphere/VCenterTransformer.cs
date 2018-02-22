using TopoMojo.Models.Virtual;

namespace TopoMojo.vSphere
{
    public class VCenterTransformer : Transformer
    {
        public string DVSuuid { get; set; }

        protected override VirtualDeviceConfigSpec GetEthernetAdapter(ref int key, Eth nic)
        {
            VirtualDeviceConfigSpec devicespec = new VirtualDeviceConfigSpec();
            VirtualEthernetCard eth = new VirtualE1000();

            if (nic.Type == "pcnet32")
                eth = new VirtualPCNet32();

            if (nic.Type == "vmx3")
                eth = new VirtualVmxnet3();

            VirtualEthernetCardDistributedVirtualPortBackingInfo ethbacking = new VirtualEthernetCardDistributedVirtualPortBackingInfo();
            ethbacking.port = new DistributedVirtualSwitchPortConnection
            {
                switchUuid = DVSuuid,
                portgroupKey = nic.Net
            };

            eth.key = key--;
            eth.backing = ethbacking;

            devicespec = new VirtualDeviceConfigSpec();
            devicespec.device = eth;
            devicespec.operation = VirtualDeviceConfigSpecOperation.add;
            devicespec.operationSpecified = true;

            return devicespec;
        }

    }
}
