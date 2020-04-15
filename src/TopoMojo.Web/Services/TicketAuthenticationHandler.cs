using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TopoMojo.Web
{
    public static class TicketAuthentication
    {
        public const string AuthenticationScheme = "Ticket";
        public const string AltSchemeName = "Bearer";
        public const string AuthorizationHeaderName = "Authorization";
        public const string ChallengeHeaderName = "WWW-Authenticate";
        public const string QuerystringField = "access_token";

        public static class ClaimNames
        {
            public const string Subject = "sub";
        }

    }
    public class TicketAuthenticationHandler : AuthenticationHandler<TicketAuthenticationOptions>
    {
        public TicketAuthenticationHandler(
            IOptionsMonitor<TicketAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMemoryCache cache
        )
            : base(options, logger, encoder, clock)
        {
            _cache = cache;
        }

        private readonly IMemoryCache _cache;

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            await Task.Delay(0);

            string key = Request.Query[TicketAuthentication.AuthenticationScheme];

            if (string.IsNullOrEmpty(key))
                key = Request.Query[TicketAuthentication.QuerystringField];

            if (string.IsNullOrEmpty(key))
            {
                string[] authHeader = Request.Headers[TicketAuthentication.AuthorizationHeaderName].ToString().Split(' ');
                string scheme = authHeader[0];
                if (authHeader.Length > 1
                    && (scheme.Equals(TicketAuthentication.AuthenticationScheme)
                    || scheme.Equals(TicketAuthentication.AltSchemeName))
                ) {
                    key = authHeader[1];
                }
            }

            if (string.IsNullOrEmpty(key))
                return AuthenticateResult.NoResult();

            _cache.TryGetValue(key, out string subject);

            _cache.Remove(key);

            if (subject == null)
                return AuthenticateResult.NoResult();

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(TicketAuthentication.ClaimNames.Subject, subject)
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
            Response.Headers[TicketAuthentication.ChallengeHeaderName] = TicketAuthentication.AuthenticationScheme;

            await base.HandleChallengeAsync(properties);
        }
    }

    public class TicketAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

}
