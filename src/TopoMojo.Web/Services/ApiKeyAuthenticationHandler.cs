using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TopoMojo.Web
{
    public static class ApiKeyAuthentication
    {
        public const string AuthenticationScheme = "ApiKey";
        public const string ApiKeyHeaderName = "x-api-key";
        public const string AuthorizationHeaderName = "Authorization";
        public const string ChallengeHeaderName = "WWW-Authenticate";

        public static class ClaimNames
        {
            public const string ClientId = "client_id";
            public const string ClientScope = "client_scope";
            public const string ClientUrl = "client_url";
        }

    }
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
        )
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            await Task.Delay(0);

            string key = Request.Headers[ApiKeyAuthentication.ApiKeyHeaderName];

            if (string.IsNullOrEmpty(key))
            {
                string[] authHeader = Request.Headers[ApiKeyAuthentication.AuthorizationHeaderName]
                    .ToString()
                    .Split(' ');

                string scheme = authHeader[0];

                if (authHeader.Length > 1
                    && scheme.Equals(ApiKeyAuthentication.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)
                ) {
                    key = authHeader[1];
                }
            }

            var client = Options.Clients.Where(c => c.Key == key).SingleOrDefault();

            if (client == null)
                return AuthenticateResult.NoResult();

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ApiKeyAuthentication.ClaimNames.ClientId, client.Id ?? "invalid"),
                        new Claim(ApiKeyAuthentication.ClaimNames.ClientScope, client.Scope ?? "public"),
                        new Claim(ApiKeyAuthentication.ClaimNames.ClientUrl, client.Url ?? "")
                    },
                    Scheme.Name
                )
            );

            return AuthenticateResult.Success(
                new AuthenticationTicket(principal, Scheme.Name)
            );
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers[ApiKeyAuthentication.ChallengeHeaderName] = ApiKeyAuthentication.AuthenticationScheme;

            await base.HandleChallengeAsync(properties);
        }
    }

    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public ICollection<ApiKeyClient> Clients { get; set; } = new List<ApiKeyClient>();
    }

    public class ApiKeyClient
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Scope { get; set; } = "public";
        public string Url { get; set; }
    }

}
