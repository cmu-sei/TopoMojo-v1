// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RepositoryStartupExtensions
    {
        public static IServiceCollection AddTopoMojoData(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> dbContextAction
        )
        {

            services.AddDbContext<TopoMojoDbContext>(dbContextAction);

            // Auto-discover from EntityStore and IEntityStore pattern
            foreach (var type in
                Assembly.GetExecutingAssembly().ExportedTypes
                .Where(t => t.Namespace == "TopoMojo.Data.EntityFrameworkCore" && t.Name.EndsWith("Store") && t.IsClass))
            {
                Type ti = type.GetInterfaces().Where(i => i.Name == $"I{type.Name}").FirstOrDefault();

                if (ti != null)
                {
                    services.AddScoped(ti, type);
                }
            }
            return services;
                // .AddDbContext<TopoMojoDbContext>(dbContextAction)
                // .AddScoped<ITemplateStore, TemplateStore>()
                // .AddScoped<IWorkspaceStore, WorkspaceStore>()
                // .AddScoped<IGamespaceStore, GamespaceStore>()
                // .AddScoped<IUserStore, UserStore>();
        }
    }
}
