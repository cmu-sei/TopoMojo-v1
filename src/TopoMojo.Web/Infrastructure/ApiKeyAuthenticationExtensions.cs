using System;
using Microsoft.AspNetCore.Authentication;
using TopoMojo.Web;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiKeyAuthenticationExtensions
    {
        public static AuthenticationBuilder AddApiKey(
            this AuthenticationBuilder builder,
            string scheme,
            Action<ApiKeyAuthenticationOptions> options
        ) {

            builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                scheme ?? ApiKeyAuthentication.AuthenticationScheme,
                options
            );

            return builder;
        }
    }
}
