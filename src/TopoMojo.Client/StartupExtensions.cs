using System;
using Polly;
using Polly.Extensions.Http;
using TopoMojo.Abstractions;
using TopoMojo.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddTopoMojoClient(
            this IServiceCollection services,
            Func<TopoMojo.Client.Options> config
        )
        {
            var options = config.Invoke();

            if (Uri.TryCreate(options.Url, UriKind.Absolute, out Uri uri))
            {
                services.AddScoped<ITopoMojoClient, TopoMojoClient>();

                services.AddHttpClient<ITopoMojoClient, TopoMojoClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Add("x-api-key", options.Key);
                })
                .AddPolicyHandler(
                    HttpPolicyExtensions.HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(options.MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                );
            }

            return services;
        }
    }
}
