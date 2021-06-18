using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo
{
    public class UserClaimsTransformation: IClaimsTransformation
    {
        private readonly IMemoryCache _cache;
        private readonly IdentityService _svc;
        private string[] exludedClaims = new string[] {
            "aud", "iss", "iat", "nbf", "exp", "aio", "c_hash", "uti", "nonce", "auth_time", "idp", "amr"
        };

        public UserClaimsTransformation(
            IMemoryCache cache,
            IdentityService svc
        )
        {
            _cache = cache;
            _svc = svc;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            FilterClaims(principal, exludedClaims);

            string subject = principal.Subject();

            if (subject.IsEmpty())
                return principal;

            if (_cache.TryGetValue<User>(subject, out User user))
            {
                return await Merge(principal, user);
            }

            user = await _svc.Load(subject);

            if (user is User)
                _cache.Set<User>(subject, user, new TimeSpan(0, 2, 0));

            return await Merge(principal, user);

        }

        private async Task<ClaimsPrincipal> Merge(ClaimsPrincipal principal, User user)
        {
            var identity = principal.Identity as ClaimsIdentity;

            AddOrUpdateClaim(identity, AppConstants.NameIdClaimName, user.Id.ToString());
            AddOrUpdateClaim(identity, AppConstants.NameClaimName, user.Name);
            AddOrUpdateClaim(identity, AppConstants.RoleClaimName, user.Role.ToString());

            foreach (var claim in identity.Claims.ToArray())
            {
                if (exludedClaims.Contains(claim.Type))
                    identity.RemoveClaim(claim);
            }

            return await Task.FromResult(principal);
        }

        private void AddOrUpdateClaim(ClaimsIdentity identity, string type, string value)
        {
            var claim = identity.FindFirst(c => c.Type == type);

            if (claim == null)
            {
                identity.AddClaim(new Claim(type, value));
            }
            else if (claim.Value != value)
            {
                identity.RemoveClaim(claim);
                identity.AddClaim(new Claim(type, value));
            }
        }

        private void FilterClaims(ClaimsPrincipal principal, string[] types)
        {
            var identity = principal.Identity as ClaimsIdentity;

            foreach (var claim in identity.Claims.ToArray())
            {
                if (types.Contains(claim.Type))
                    identity.RemoveClaim(claim);
            }
        }
    }
}
