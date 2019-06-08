// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TopoMojo.Core;
using TopoMojo.Core.Mappers;
using TopoMojo.Core.Privileged;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddTopoMojo(
            this IServiceCollection services,
            Func<IConfigurationSection> coreConfig
        ) {

            return services
                .AddOptions().Configure<CoreOptions>(coreConfig())
                .AddScoped(sp => sp.GetService<IOptionsMonitor<CoreOptions>>().CurrentValue)
                .AddScoped<TopologyManager>()
                .AddScoped<TemplateManager>()
                .AddScoped<GamespaceManager>()
                .AddScoped<ProfileManager>()
                .AddScoped<ChatService>()
                .AddScoped<TransferService>()
                .AddScoped<ProfileService>()
                .AddMappers();
        }
    }
}
