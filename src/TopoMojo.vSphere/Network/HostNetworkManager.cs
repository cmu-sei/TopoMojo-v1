using System.Collections.Generic;
using System.Threading.Tasks;
using TopoMojo.Models.Virtual;
using TopoMojo.vSphere.Helpers;

namespace TopoMojo.vSphere.Network
{
    public class HostNetworkManager : NetworkManager
    {
        public HostNetworkManager(
            Settings settings,
            Dictionary<string, Vm> vmCache,
            VlanManager vlanManager
        ) : base(settings, vmCache, vlanManager)
        {

        }

        public override Task<PortGroupAllocation> AddPortGroup(string sw, Eth eth)
        {
            throw new System.NotImplementedException();
        }

        public override Task AddSwitch(string sw)
        {
            throw new System.NotImplementedException();
        }

        public override Task<VmNetwork[]> GetVmNetworks(ManagedObjectReference managedObjectReference)
        {
            throw new System.NotImplementedException();
        }

        public override Task<PortGroupAllocation[]> LoadPortGroups()
        {
            throw new System.NotImplementedException();
        }

        public override Task RemovePortgroup(string pgReference)
        {
            throw new System.NotImplementedException();
        }

        public override Task RemoveSwitch(string sw)
        {
            throw new System.NotImplementedException();
        }
    }
}
