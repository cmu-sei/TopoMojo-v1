using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using TopoMojo;
using TopoMojo.Api;

namespace  Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationStartupExtensions
    {
        public static IServiceCollection AddConfiguredAuthorization(
            this IServiceCollection services
        ) {
            services.AddAuthorization(_ =>
            {
                _.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme,
                        ApiKeyAuthentication.AuthenticationScheme,
                        AppConstants.CookieScheme
                    ).Build()
                ;

                _.AddPolicy(AppConstants.AdminOnlyPolicy, new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme,
                        ApiKeyAuthentication.AuthenticationScheme,
                        AppConstants.CookieScheme
                    )
                    .RequireClaim(AppConstants.RoleClaimName, TopoMojo.Api.Models.UserRole.Administrator.ToString())
                    .Build()
                );

                _.AddPolicy(AppConstants.TicketOnlyPolicy, new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(TicketAuthentication.AuthenticationScheme)
                    .Build()
                );
                _.AddPolicy(AppConstants.CookiePolicy, new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(AppConstants.CookieScheme)
                    .Build()
                );

                // _.AddPolicy("Players", new AuthorizationPolicyBuilder()
                //     .RequireAuthenticatedUser()
                //     .AddAuthenticationSchemes(
                //         JwtBearerDefaults.AuthenticationScheme,
                //         TicketAuthentication.AuthenticationScheme,
                //         AppConstants.CookieScheme
                //     )
                //     .Build());

                // _.AddPolicy("TicketOrCookie", new AuthorizationPolicyBuilder()
                //     .RequireAuthenticatedUser()
                //     .AddAuthenticationSchemes(
                //         AppConstants.CookieScheme,
                //         TicketAuthentication.AuthenticationScheme
                //     )
                //     .Build());
            });

            return services;
        }
    }

}
