using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TopoMojo;
using TopoMojo.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TopoMojoStartupExtentions
    {

        public static IServiceCollection AddTopoMojo(
            this IServiceCollection services,
            CoreOptions options
        ) {

            services.AddSingleton<CoreOptions>(_ => options);

            // Auto-discover from EntityService pattern
            foreach (var t in Assembly
                .GetExecutingAssembly()
                .ExportedTypes
                .Where(t => t.Namespace == "TopoMojo.Services"
                    && t.Name.EndsWith("Service")
                    && t.IsClass
                    && !t.IsAbstract
                )
            )
            {
                services.AddScoped(t);
            }

            return services;
        }

        public static IMapperConfigurationExpression AddTopoMojoMaps(
            this IMapperConfigurationExpression cfg
        )
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
            return cfg;
        }

        public static IServiceCollection AddTopoMojoData(
            this IServiceCollection services,
            string provider,
            string connstr,
            string migrationAssembly = null
        )
        {

            if (string.IsNullOrEmpty(migrationAssembly))
                migrationAssembly = Assembly.GetExecutingAssembly().GetName().Name;

            switch (provider.ToLower())
            {

                case "sqlserver":
                // builder.Services.AddEntityFrameworkSqlServer();
                services.AddDbContext<TopoMojoDbContext, TopoMojoDbContextSqlServer>(
                    db => db.UseSqlServer(connstr, options => options.MigrationsAssembly(migrationAssembly))
                );
                break;

                case "postgresql":
                // services.AddEntityFrameworkNpgsql();
                services.AddDbContext<TopoMojoDbContext, TopoMojoDbContextPostgreSQL>(
                    db => db.UseNpgsql(connstr, options => options.MigrationsAssembly(migrationAssembly))
                );
                break;

                default:
                // services.AddEntityFrameworkInMemoryDatabase();
                services.AddDbContext<TopoMojoDbContext, TopoMojoDbContextInMemory>(
                    db => db.UseInMemoryDatabase(connstr)
                );
                break;

            }

            // Auto-discover from EntityStore and IEntityStore pattern
            foreach (var type in Assembly
                .GetExecutingAssembly()
                .ExportedTypes
                .Where(t =>
                    t.Namespace == "TopoMojo.Data"
                    && t.Name.EndsWith("Store")
                    && t.IsClass
                    && !t.IsAbstract
                )
            )
            {
                Type ti = type.GetInterfaces().Where(i => i.Name == $"I{type.Name}").FirstOrDefault();

                if (ti != null)
                {
                    services.AddScoped(ti, type);
                }
            }

            return services;
        }
    }
}
