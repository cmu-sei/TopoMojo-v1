using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TopoMojo.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationOptions(
            this IServiceCollection services,
            IConfiguration config
        ) {
              return services.AddOptions()

                .Configure<ControlOptions>(config.GetSection("Control"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<ControlOptions>>().CurrentValue)

                .Configure<ClientSettings>(config.GetSection("ClientSettings"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<ClientSettings>>().CurrentValue)

                .Configure<FileUploadOptions>(config.GetSection("FileUpload"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<FileUploadOptions>>().CurrentValue);

        }

        public static IServiceCollection AddProfileResolver(
            this IServiceCollection services
        ) {
            return services.AddMemoryCache()
                .AddScoped<Services.ProfileResolver>()
                .AddScoped<Core.Abstractions.IProfileResolver>(sp => sp.GetService<Services.ProfileResolver>())
                .AddScoped<IClaimsTransformation>(sp => sp.GetService<Services.ProfileResolver>())
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        }
    }
}