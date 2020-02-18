// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TopoMojo.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddTopoMojo(
            this IServiceCollection services,
            Func<IConfigurationSection> coreConfig
        ) {
            services
                .AddOptions().Configure<CoreOptions>(coreConfig())
                .AddScoped(sp => sp.GetService<IOptionsMonitor<CoreOptions>>().CurrentValue);

            // Auto-discover from EntityService pattern
            foreach (var t in
                Assembly.GetExecutingAssembly().ExportedTypes
                .Where(t => t.Namespace == "TopoMojo.Core"
                    && t.Name.EndsWith("Service")
                    && t.IsClass))
            {
                services.AddScoped(t);
            }


                // .AddScoped<WorkspaceService>()
                // .AddScoped<TemplateService>()
                // .AddScoped<GamespaceService>()
                // .AddScoped<UserService>()
                // .AddScoped<ChatService>()
                // .AddScoped<TransferService>()
                // .AddScoped<IdentityService>()
                // .AddScoped<EngineService>()
                // .AddMappers();

            return services;
        }

        public static IMapperConfigurationExpression AddTopoMojoMaps(
            this IMapperConfigurationExpression cfg
        )
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
            return cfg;
        }
    }
}
