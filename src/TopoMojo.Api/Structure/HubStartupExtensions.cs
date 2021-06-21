using Microsoft.AspNetCore.SignalR;
using TopoMojo.Web.Controllers;
using TopoMojo.Web.Services;

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
