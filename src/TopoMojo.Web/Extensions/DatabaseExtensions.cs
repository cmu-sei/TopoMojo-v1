// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TopoMojo.Data.Entities;
using TopoMojo.Data.EntityFrameworkCore;
using TopoMojo.Models;

namespace TopoMojo.Extensions
{
    public static class DatabaseStartupExtensions
    {

        public static IServiceCollection AddDbProvider(
            this IServiceCollection services,
            Func<IConfigurationSection> coreConfig
        ) {
            IConfigurationSection config = coreConfig();
            string dbProvider = config.GetValue<string>("Provider", "Sqlite").Trim();
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
                .Configure<DatabaseOptions>(config)
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

                if (options.DevModeRecreate)
                {
                    topoDb.Database.EnsureDeleted();
                }

                topoDb.Database.Migrate();

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
                    }
                    topoDb.SaveChanges();
                }

                return webHost;
            }
        }
    }
}
