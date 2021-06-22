using Microsoft.AspNetCore.SignalR;
using TopoMojo.Api.Hubs;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HubStartupExtensions
    {
        public static IServiceCollection AddSignalRHub(
            this IServiceCollection services
        ) {
            services.AddSignalR(options => {});

            services
                .AddSingleton<HubCache>()
                .AddSingleton<IUserIdProvider, SubjectProvider>()
            ;

            return services;
        }

    }

}
