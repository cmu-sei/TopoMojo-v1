using System;
using System.Linq;
using Jam.Accounts;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TopoMojo.Data;
using TopoMojo.Data.Entities;

namespace TopoMojo.Extensions
{
    public static class DatabaseStartupExtensions
    {
        public static DbContextOptionsBuilder UseConfiguredDatabase(
            this DbContextOptionsBuilder builder,
            DatabaseOptions dbOptions
        )
        {
            switch (dbOptions.Provider)
            {
                case "Sqlite":
                builder.UseSqlite(
                    dbOptions.ConnectionString,
                    options => options.MigrationsAssembly(dbOptions.MigrationsAssembly)
                );
                break;

                case "SqlServer":
                builder.UseSqlServer(
                    dbOptions.ConnectionString,
                    options => options.MigrationsAssembly(dbOptions.MigrationsAssembly)
                );
                break;
            }
            return builder;
        }

        public static IApplicationBuilder InitializeDatabase(this IApplicationBuilder app, DatabaseOptions dbOptions, bool isDevelopment)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                TopoMojoDbContext topoDb = scope.ServiceProvider.GetRequiredService<TopoMojoDbContext>();
                AccountDbContext accountDb = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
                if (isDevelopment && dbOptions.DevModeRecreate)
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
                    IAccountRepository datarepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
                    IAccountManager mgr = new AccountManager(
                        datarepo,
                        new AccountOptions(),
                        scope.ServiceProvider.GetRequiredService<ILoggerFactory>(),
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
            }

            return app;

        }
    }
}