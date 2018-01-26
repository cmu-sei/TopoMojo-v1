using System;
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
                //services.AddEntityFrameworkSqlServer();
                break;

                case "PostgreSQL":
                //services.AddEntityFrameworkNpgsql();
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
            //var migrationsAssembly = String.Format("{0}.Migrations.{1}", typeof(Startup).GetTypeInfo().Assembly.GetName().Name, dbProvider);
            var migrationsAssembly = String.Format("{0}", typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            var connectionString = config.GetConnectionString(dbProvider);

            switch (dbProvider)
            {
                case "Sqlite":
                builder.UseSqlite(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
                break;

                case "SqlServer":
                //builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
                break;

                case "PostgreSQL":
                //builder.UseNpgsql(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
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
                DatabaseOptions options = services.GetService<DatabaseOptions>();
                TopoMojoDbContext topoDb = services.GetService<TopoMojoDbContext>();
                AccountDbContext accountDb = services.GetService<AccountDbContext>();
                if (options.DevModeRecreate)
                {
                    topoDb.Database.EnsureDeleted();
                    accountDb.Database.EnsureDeleted();
                }

                topoDb.Database.Migrate();
                accountDb.Database.Migrate();

                string guid = Guid.NewGuid().ToString();
                if (!accountDb.Accounts.Any())
                {
                    //IAccountManager mgr = scope.ServiceProvider.GetRequiredService<IAccountManager>();
                    IAccountRepository datarepo = services.GetRequiredService<IAccountRepository>();
                    IAccountManager mgr = new AccountManager(
                        datarepo,
                        new AccountOptions(),
                        services.GetRequiredService<ILoggerFactory>(),
                        null, null, null);
                        mgr.RegisterWithCredentialsAsync(
                            new Credentials {
                                Username = "admin@this.ws",
                                Password = "321ChangeMe!"
                            }, guid).Wait();
                }

                if (!topoDb.Profiles.Any())
                {
                    topoDb.Profiles.Add(new Profile {
                        GlobalId = guid,
                        Name = "Administrator",
                        WhenCreated = DateTime.UtcNow,
                        IsAdmin = true
                    });
                    topoDb.SaveChanges();
                }

                return webHost;
            }
        }

    }
}