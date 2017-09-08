using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface ITopologyRepository : IRepository<Topology>
    {
        Task<Topology> FindByShareCode(string code);
    }
}