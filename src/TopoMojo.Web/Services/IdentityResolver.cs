// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Services
{
    public class IdentityResolver : IIdentityResolver, IClaimsTransformation
    {
        public IdentityResolver(
            IHttpContextAccessor context,
            IMemoryCache cache,
            IdentityService identitySvc,
            ControlOptions options
        ){
            _context = context;
            _cache = cache;
            _identitySvc = identitySvc;

            _cacheOptions = new MemoryCacheEntryOptions();
            _cacheOptions.SetAbsoluteExpiration(
                TimeSpan.FromSeconds(options.ProfileCacheSeconds)
            );
        }

        private readonly IHttpContextAccessor _context;
        private readonly IMemoryCache _cache;
        private readonly IdentityService _identitySvc;
        private readonly MemoryCacheEntryOptions _cacheOptions;
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
            {
                return principal;
            }

            string sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            string name = principal.FindFirstValue("name");

            if (_cache.TryGetValue(sub, out _user))
                return principal.AddUserClaims(_user);


            // prevents auto-registration from happening more than once
            await semaphoreSlim.WaitAsync();

            try
            {
                if (!_cache.TryGetValue(sub, out _user))
                {
                    //not in cache, so fetch
                    _user = await _identitySvc.FindByGlobalId(sub);

                    if (_user == null)
                    {
                        // Auto-Register
                        try
                        {
                            _user = await _identitySvc.Add(new User {
                                GlobalId = sub,
                                Name = name
                            });
                        }
                        catch (Exception ex)
                        {
                            // concurrency issue, maybe solved by semaphore; just refetch
                            _user = await _identitySvc.FindByGlobalId(sub);
                            if (_user == null)
                            {
                                throw ex;
                            }
                        }
                    }

                    _cache.Set(sub, _user, _cacheOptions);
                }

            }
            finally
            {
                semaphoreSlim.Release();
            }

            return principal.AddUserClaims(_user);
        }
    }
}
