// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using AutoMapper;
using TopoMojo.Core.Mappers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupMapperExtensions
    {
        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            Mapper.Initialize(cfg => {
                cfg.AddProfile<ActorProfile>();
                cfg.AddProfile<WorkspaceProfile>();
                cfg.AddProfile<TemplateProfile>();
                cfg.AddProfile<GamespaceProfile>();
                cfg.AddProfile<MessageProfile>();
            });
            return services;

        }
    }
}
