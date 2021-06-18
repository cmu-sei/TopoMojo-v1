using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using TopoMojo;
using TopoMojo.Web;

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
                        JwtBearerDefaults.AuthenticationScheme
                    ).Build();

                _.AddPolicy("AdminOnly", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme
                    )
                    .RequireClaim(AppConstants.RoleClaimName, TopoMojo.Models.UserRole.Administrator.ToString())
                    .Build());

                _.AddPolicy("TrustedClients", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(ApiKeyAuthentication.AuthenticationScheme)
                    .Build());

                _.AddPolicy("OneTimeTicket", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(TicketAuthentication.AuthenticationScheme)
                    .Build());

                _.AddPolicy("Players", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme,
                        TicketAuthentication.AuthenticationScheme,
                        AppConstants.CookieScheme
                    )
                    .Build());

                _.AddPolicy("TicketOrCookie", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        AppConstants.CookieScheme,
                        TicketAuthentication.AuthenticationScheme
                    )
                    .Build());
            });

            return services;
        }
    }

}
