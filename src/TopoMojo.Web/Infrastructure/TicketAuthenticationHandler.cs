using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
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
            IDataProtectionProvider dataProtection
        )
            : base(options, logger, encoder, clock)
        {
            _dp = dataProtection.CreateProtector($"_dp:{Assembly.GetEntryAssembly().FullName}");
        }

        private readonly IDataProtector _dp;

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

            string[] value = _dp.Unprotect(key).Split(
                new char[] { ' ', ';', ',', '|'},
                StringSplitOptions.RemoveEmptyEntries
            );

            bool expired = Int64.TryParse(value.First(), out long ticks)
                ? DateTime.UtcNow.Ticks > ticks
                : true;

            string subject = Guid.TryParse(value.Last(), out Guid guid)
                ? guid.ToString()
                : "";

            if (subject == null || expired)
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
