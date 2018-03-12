using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface IProfileRepository : IRepository<Profile>
    {
        Task<Profile> LoadDetail(int id);
        Task<bool> CanEditSpace(string globalId, Profile profile);
    }
}