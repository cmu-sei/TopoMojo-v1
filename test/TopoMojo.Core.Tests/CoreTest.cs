using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TopoMojo.Core;
using TopoMojo.Abstractions;
using TopoMojo.Models;

namespace Tests
{
    public class CoreTest
    {
        public CoreTest()
        {
            Initialize();
        }

        protected IUserResolver _ur = null;
        protected const string _complexPassword = "~Tartans@1~";
        protected IOptions<CoreOptions> _optAccessor = null;
        protected CoreOptions _options = null;

        protected Person _user = null;
        protected ILoggerFactory _mill = null;
        private DbContextOptions<TopoMojoDbContext> _dbOptions;

        protected TestSession CreateSession()
        {
            return new TestSession(
                CreateContext(),
                _options,
                _mill
            );
        }

        protected TopoMojoDbContext CreateContext()
        {
            return new TopoMojoDbContext(_dbOptions);
        }

        protected void Initialize()
        {
            _options = new CoreOptions();
            _optAccessor = Options.Create(_options);

            _mill = new LoggerFactory();
            _mill.AddConsole();
            _mill.AddDebug();

            _dbOptions = new DbContextOptionsBuilder<TopoMojoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (TopoMojoDbContext ctx = new TopoMojoDbContext(_dbOptions))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
                _user = new Person { Id = 1, Name = "tester" }; //AddTestUser("tester@step.local");
                _ur = new UserResolver(_user);
            }

        }

        // protected Person AddTestUser(string name)
        // {
        //     using (TopoMojoDbContext ctx = new TopoMojoDbContext(_dbOptions))
        //     {
        //         UserManager userManager = new UserManager(ctx, Options.Create(_options.Identity), Mill, null);
        //         Person person = userManager.FindByAccountAsync(name).Result;
        //         if (person == null)
        //         {
        //             person = userManager.RegisterWithCredentialsAsync(name, _complexPassword).Result;
        //         }
        //         ctx.Entry(person).Reference(o => o.Profile).Load();
        //         return person;
        //     }
        // }


    }
}