using System.Threading.Tasks;
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
    }
}