using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;

namespace TopoMojo.Core
{
    public class ProfileManager : EntityManager<Person>
    {
        public ProfileManager
        (
            IServiceProvider sp
        ) : base (sp)
        {
        }

        public async Task<Person> LoadByGlobalId(string globalId)
        {
            return await _db.People
                .Where(p => p.GlobalId == globalId)
                .SingleOrDefaultAsync();
        }
    }
}