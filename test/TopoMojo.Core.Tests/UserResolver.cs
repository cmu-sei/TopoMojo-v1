using System.Threading.Tasks;
using TopoMojo.Abstractions;
using TopoMojo.Core;

namespace Tests
{
    public class UserResolver : IUserResolver
    {
        public UserResolver(Person user)
        {
            _user = user;
        }

        protected readonly Person _user;

        public async Task<Person> GetCurrentUserAsync()
        {
            await Task.Delay(0); //hack to prevent compiler warnings
            return _user;
        }
    }
}