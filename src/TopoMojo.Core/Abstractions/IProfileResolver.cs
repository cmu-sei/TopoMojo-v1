using System.Security.Principal;
using System.Threading.Tasks;
using TopoMojo.Core.Models;

namespace TopoMojo.Core.Abstractions
{
    public interface IProfileResolver
    {
        Profile Profile { get; }
    }
}