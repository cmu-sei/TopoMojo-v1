// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
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

            return services
                .AddOptions().Configure<CoreOptions>(coreConfig())
                .AddScoped(sp => sp.GetService<IOptionsMonitor<CoreOptions>>().CurrentValue)
                .AddScoped<WorkspaceService>()
                .AddScoped<TemplateService>()
                .AddScoped<GamespaceService>()
                .AddScoped<UserService>()
                .AddScoped<ChatService>()
                .AddScoped<TransferService>()
                .AddScoped<PrivilegedUserService>()
                .AddScoped<EngineService>()
                .AddMappers();
        }
    }
}
