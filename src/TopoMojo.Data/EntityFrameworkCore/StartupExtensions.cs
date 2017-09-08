using TopoMojo.Data.Abstractions;
using TopoMojo.Data.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped<ITemplateRepository, TemplateRepository>()
                .AddScoped<ITopologyRepository, TopologyRepository>()
                .AddScoped<IProfileRepository, ProfileRepository>();

        }
    }
}