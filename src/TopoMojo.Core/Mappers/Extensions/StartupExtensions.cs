using AutoMapper;
using TopoMojo.Core.Mappers;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupMapperExtensions
    {
        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            Mapper.Initialize(cfg => {
                cfg.AddProfile<ActorProfile>();
                cfg.AddProfile<TopologyProfile>();
                cfg.AddProfile<TemplateProfile>();
                cfg.AddProfile<GamespaceProfile>();
                cfg.AddProfile<MessageProfile>();
            });
            return services;

        }
    }
}