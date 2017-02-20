using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using TopoMojo.Data;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using Microsoft.Extensions.Logging;
//using TopoMojo.Core;

namespace TopoMojo.Services
{
    public class UserResolver : IUserResolver
    {
        public UserResolver(
            IHttpContextAccessor context,
            UserManager<ApplicationUser> userManager,
            TopoMojoDbContext db,
            ILoggerFactory mill)
        {
            _context = context;
            _userManager = userManager;
            _db = db;
            _logger = mill.CreateLogger<UserResolver>();
        }

        private readonly IHttpContextAccessor _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TopoMojoDbContext _db;
        private readonly ILogger<UserResolver> _logger;

        public async Task<Person> GetCurrentUserAsync()
        {

            _logger.LogDebug(_context.HttpContext.User.ToString());
            ApplicationUser user = await _userManager.GetUserAsync(_context.HttpContext.User);
            Person person = await _db.People.FindAsync(user.PersonId);
            return person;
            //return new Person { Id = 1, Name = "developer@this.ws", IsAdmin = true };
        }
    }

    public class DirectUserResolver : IUserResolver
    {
        public DirectUserResolver(Person person)
        {
            _person = person;
        }

        private readonly Person _person;

        public async Task<Person> GetCurrentUserAsync()
        {
            await Task.Delay(0);
            return _person;
        }
    }
}