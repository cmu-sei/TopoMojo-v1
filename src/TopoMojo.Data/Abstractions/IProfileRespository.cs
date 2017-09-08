using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface IProfileRepository : IRepository<Profile>
    {
        Task<Profile> LoadOrCreate(Profile profile);
    }
}