using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using TopoMojo;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationStartupExtensions
    {
        public static IServiceCollection AddConfiguredAuthentication(
            this IServiceCollection services,
            OidcOptions oidc,
            ICollection<ApiKeyClient> clients
        ) {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddScoped<IClaimsTransformation, UserClaimsTransformation>()

                .AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })

                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Audience = oidc.Audience;
                    options.Authority = oidc.Authority;
                    options.RequireHttpsMetadata = oidc.RequireHttpsMetadata;
                })

                .AddApiKey(ApiKeyAuthentication.AuthenticationScheme, options =>
                {
                    options.Clients = clients;
                })

                .AddTicketAuthentication(TicketAuthentication.AuthenticationScheme, options => {})

                .AddCookie(AppConstants.CookieScheme, opt =>
                {
                    // opt.ExpireTimeSpan = new TimeSpan(4, 0, 0);
                    // opt.SlidingExpiration = true;
                    opt.Cookie = new CookieBuilder
                    {
                        Name = AppConstants.CookieScheme
                    };
                })
            ;

            return services;
        }
    }
}
