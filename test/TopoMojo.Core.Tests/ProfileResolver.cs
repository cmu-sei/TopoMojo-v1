using System.Threading.Tasks;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models;

namespace Tests
{
    public class ProfileResolver : IProfileResolver
    {
        public ProfileResolver(Profile user)
        {
            Profile = user;
        }

        public Profile Profile { get; private set;}

    }
}