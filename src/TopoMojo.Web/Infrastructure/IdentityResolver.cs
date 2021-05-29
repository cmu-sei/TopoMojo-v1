// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Services
{
    public class IdentityResolver : IIdentityResolver, IClaimsTransformation
    {
        public IdentityResolver(
            IdentityService identitySvc
        ){
            _identitySvc = identitySvc;
        }

        private readonly IdentityService _identitySvc;
        private User _user = new User();
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);
        private string[] exludedClaims = new string[] {
            "aud", "iss", "iat", "nbf", "exp", "aio", "c_hash", "uti", "nonce", "auth_time", "idp", "amr"
        };

        public User User { get { return _user; } }
        public Client Client { get; set; }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;

            foreach (var claim in identity.Claims.ToArray())
            {
                if (exludedClaims.Contains(claim.Type))
                    identity.RemoveClaim(claim);
            }

            this.Client = new Client
            {
                Id = principal.FindFirstValue(ApiKeyAuthentication.ClaimNames.ClientId),
                Scope = principal.FindFirstValue(ApiKeyAuthentication.ClaimNames.ClientScope),
                Url = principal.FindFirstValue(ApiKeyAuthentication.ClaimNames.ClientUrl)
            };

            if (principal.Identity.AuthenticationType == ApiKeyAuthentication.AuthenticationScheme)
                return principal;

            string sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            string name = principal.FindFirstValue(AppConstants.NameClaimName);

            _user = await _identitySvc.Load(sub) ?? new User
            {
                GlobalId = sub,
                Name = name
            };

            AddOrUpdateClaim(identity, JwtRegisteredClaimNames.NameId, _user.Id.ToString());
            AddOrUpdateClaim(identity, AppConstants.NameClaimName, _user.Name);
            AddOrUpdateClaim(identity, AppConstants.RoleClaimName, _user.Role.ToString());

            return principal;
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
    }
}
