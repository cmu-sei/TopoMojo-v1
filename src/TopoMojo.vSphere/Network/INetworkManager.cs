// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using NetVimClient;
using TopoMojo.Models.Virtual;
using TopoMojo.vSphere.Helpers;

namespace TopoMojo.vSphere.Network
{
    public interface INetworkManager
    {
        //props: vim, pool, net, dvs, dvsuuid

        Task AddSwitch(string sw);
        Task RemoveSwitch(string sw);
        Task<PortGroupAllocation> AddPortGroup(string sw, Eth eth);
        Task RemovePortgroup(string pgReference);
        Task<VmNetwork[]> GetVmNetworks(ManagedObjectReference managedObjectReference);
        Task<PortGroupAllocation[]> LoadPortGroups();

        Task Initialize();
        Task Provision(Template template);
        Task Unprovision(ManagedObjectReference vmMOR);
        Task Clean();
        string Resolve(string net);
    }
}
