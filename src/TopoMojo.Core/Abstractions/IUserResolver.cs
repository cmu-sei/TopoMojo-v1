using System.Threading.Tasks;
using TopoMojo.Core;

namespace TopoMojo.Abstractions
{
    public interface IUserResolver
    {
        Task<Person> GetCurrentUserAsync();
    }
}