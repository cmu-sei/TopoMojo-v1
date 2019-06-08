// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
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
            return services
                .AddDbContext<TopoMojoDbContext>(dbContextAction)
                .AddScoped<ITemplateRepository, TemplateRepository>()
                .AddScoped<ITopologyRepository, TopologyRepository>()
                .AddScoped<IGamespaceRepository, GamespaceRepository>()
                .AddScoped<IProfileRepository, ProfileRepository>();
        }
    }
}
