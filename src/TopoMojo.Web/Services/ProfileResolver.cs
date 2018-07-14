using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models;
using TopoMojo.Core.Privileged;
using TopoMojo.Data.EntityFrameworkCore;

namespace TopoMojo.Services
{
    //Transforms ClaimsPrincipal to TopoMojo.Core.Models.Profile
    //(Once per request, if AddScoped())
    public class ProfileResolver : IProfileResolver, IClaimsTransformation
    {
        public ProfileResolver(
            IHttpContextAccessor context,
            IMemoryCache cache,
            ProfileService repo,
            ControlOptions options
        ){
            _context = context;
            _cache = cache;
            _repo = repo;
            _cacheOptions = new MemoryCacheEntryOptions();
            _cacheOptions.SetAbsoluteExpiration(
                TimeSpan.FromSeconds(options.ProfileCacheSeconds)
            );
        }

        private readonly IHttpContextAccessor _context;
        private readonly IMemoryCache _cache;
        private readonly ProfileService _repo;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private Profile _profile = null;

        public Profile Profile {
            get
            {
                if (_profile == null)
                    _profile = BuildProfileModel(_context.HttpContext.User);
                return _profile;
            }
        }

        private Profile BuildProfileModel(ClaimsPrincipal principal)
        {
            var profile = new Profile();
            profile.GlobalId = principal.FindFirst(JwtClaimTypes.Subject)?.Value;
            profile.Name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? "Anonymous";
            profile.IsAdmin = principal.IsInRole("admin");

            if (Int32.TryParse(principal.FindFirst(JwtRegisteredClaimNames.NameId)?.Value, out int id))
                profile.Id = id;

            if (Int32.TryParse(principal.FindFirst("workspacelimit")?.Value, out int limit))
                profile.WorkspaceLimit = limit;

            return profile;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            //if cached, add local claims from cache
            //else fetch local claims from db (and add to cache)
            //add local claims to principal

            //only run this once per scope
            if (_profile != null)
                return principal;

            string sub = principal.FindFirstValue(JwtClaimTypes.Subject);
            if (!_cache.TryGetValue(sub, out List<Claim> claims))
            {
                claims = new List<Claim>();
                var profile = await _repo.FindByGlobalId(sub);
                if (profile != null)
                {
                    if (profile.IsAdmin)
                        claims.Add(new Claim(JwtClaimTypes.Role, "admin"));

                    string name = principal.FindFirstValue(JwtClaimTypes.Name);
                    if (!String.IsNullOrEmpty(name) && name != profile.Name)
                    {
                        profile.Name = name;
                        await _repo.Update(profile);
                    }
                }
                else
                {
                    profile = await _repo.Add(BuildProfileModel(principal));
                }
                claims.Add(new Claim(JwtRegisteredClaimNames.NameId, profile.Id.ToString()));
                claims.Add(new Claim("workspacelimit", profile.WorkspaceLimit.ToString()));
                _cache.Set(sub, claims, _cacheOptions);
            }

            ((ClaimsIdentity)principal.Identity).AddClaims(claims);
            _profile = BuildProfileModel(principal);
            return principal;
        }
    }
}
