using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Jam.Accounts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TopoMojo.Data.Entities;
using TopoMojo.Data.EntityFrameworkCore;

namespace TopoMojo.Extensions
{
    public static class DatabaseStartupExtensions
    {

        public static IServiceCollection AddDbProvider(
            this IServiceCollection services,
            IConfiguration config
        ) {
            string dbProvider = config.GetValue<string>("Database:Provider", "Sqlite").Trim();
            switch (dbProvider)
            {
                case "Sqlite":
                services.AddEntityFrameworkSqlite();
                break;

                case "SqlServer":
                services.AddEntityFrameworkSqlServer();
                break;

                case "PostgreSQL":
                services.AddEntityFrameworkNpgsql();
                break;

            }

            return services
                .AddOptions()
                .Configure<DatabaseOptions>(config.GetSection("Database"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<DatabaseOptions>>().CurrentValue);
        }

        public static DbContextOptionsBuilder UseConfiguredDatabase(
            this DbContextOptionsBuilder builder,
            IConfiguration config
        )
        {
            string dbProvider = config.GetValue<string>("Database:Provider", "Sqlite").Trim();
            // var migrationsAssembly = String.Format("{0}.Migrations.{1}", typeof(Startup).GetTypeInfo().Assembly.GetName().Name, dbProvider);
            // var migrationsAssembly = String.Format("{0}", typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            var migrationsAssembly = String.Format("TopoMojo.Data.{0}", dbProvider);
            var connectionString = config.GetConnectionString(dbProvider);

            switch (dbProvider)
            {
                case "Sqlite":
                builder.UseSqlite(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
                break;

                case "SqlServer":
                builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
                break;

                case "PostgreSQL":
                builder.UseNpgsql(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
                break;

            }
            return builder;
        }

        public static IWebHost InitializeDatabase(
            this IWebHost webHost
        )
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                IConfiguration config = services.GetRequiredService<IConfiguration>();
                IHostingEnvironment env = services.GetService<IHostingEnvironment>();
                DatabaseOptions options = services.GetService<DatabaseOptions>();
                TopoMojoDbContext topoDb = services.GetService<TopoMojoDbContext>();
                AccountDbContext accountDb = services.GetService<AccountDbContext>();
                IAccountRepository datarepo = services.GetRequiredService<IAccountRepository>();
                IAccountManager mgr = new AccountManager(
                    datarepo,
                    new AccountOptions(),
                    services.GetRequiredService<ILoggerFactory>(),
                    null, null, null);

                if (options.DevModeRecreate)
                {
                    topoDb.Database.EnsureDeleted();
                    accountDb.Database.EnsureDeleted();
                }

                topoDb.Database.Migrate();
                accountDb.Database.Migrate();

                string seedFile = Path.Combine(env.ContentRootPath, options.SeedTemplateKey);
                if (File.Exists(seedFile)) {
                    DbSeedModel seedData = JsonConvert.DeserializeObject<DbSeedModel>(File.ReadAllText(seedFile));
                    foreach (var u in seedData.Users)
                    {
                        if (!topoDb.Profiles.Any(p => p.GlobalId == u.GlobalId))
                        {
                            topoDb.Profiles.Add(new Profile
                            {
                                Name = u.Name,
                                GlobalId = u.GlobalId,
                                WhenCreated = DateTime.UtcNow,
                                IsAdmin = u.IsAdmin
                            });
                        }

                        if (
                            u.Password.HasValue() &&
                            !(accountDb.Accounts.Any(a => a.GlobalId == u.GlobalId))
                        )
                        {
                            mgr.RegisterWithCredentialsAsync(
                            new Credentials {
                                Username = u.Username,
                                Password = u.Password
                            }, u.GlobalId).Wait();
                        }
                    }
                    topoDb.SaveChanges();
                }

                return webHost;
            }
        }

    }

    public class DbSeedModel
    {
        public DbSeedUser[] Users { get; set; } = new DbSeedUser[] {};
    }

    public class DbSeedUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
    }
}