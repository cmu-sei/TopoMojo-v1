using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TopoMojo.Models.Virtual;
using TopoMojo.vSphere.Helpers;

namespace TopoMojo.vSphere.Network
{
    public class DistributedNetworkManager : NetworkManager
    {
        public DistributedNetworkManager(
            Settings settings,
            ConcurrentDictionary<string, Vm> vmCache,
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
            // foreach (var dvpg in clunkyTree.FindType("DistributedVirtualPortgroup"))
            // {
            //     var config = (DVPortgroupConfigInfo)dvpg.GetProperty("config");
            //     if (config.distributedVirtualSwitch.Value == _dvs.Value)
            //     {
            //         string net = dvpg.GetProperty("name") as string;
            //         if (config.defaultPortConfig is VMwareDVSPortSetting
            //             && ((VMwareDVSPortSetting)config.defaultPortConfig).vlan is VmwareDistributedVirtualSwitchVlanIdSpec)
            //         {
            //             _pgAllocation.Add(
            //                 net,
            //                 new PortGroupAllocation
            //                 {
            //                     Net = net,
            //                     Key = dvpg.obj.AsString(),
            //                     VlanId = ((VmwareDistributedVirtualSwitchVlanIdSpec)((VMwareDVSPortSetting)config.defaultPortConfig).vlan).vlanId
            //                 }
            //             );
            //         }
            //     }
            // }
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
