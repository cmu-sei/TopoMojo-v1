using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models;
using TopoMojo.Core.Privileged;

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
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);

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
            profile.GlobalId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            profile.Name = principal.FindFirst("name")?.Value ?? "Anonymous";
            profile.IsAdmin = principal.IsInRole("Administrator");
            profile.Role = principal.FindFirst("role")?.Value ?? "User";

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

            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released
            await semaphoreSlim.WaitAsync();
            try
            {
                string sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (!_cache.TryGetValue(sub, out List<Claim> claims))
                {

                    claims = new List<Claim>();
                    var profile = await _repo.FindByGlobalId(sub);
                    if (profile != null)
                    {
                        string name = principal.FindFirstValue("name");
                        if (!String.IsNullOrEmpty(name) && name != profile.Name)
                        {
                            profile.Name = name;
                            await _repo.Update(profile);
                        }
                    }
                    else
                    {
                        try
                        {
                            profile = await _repo.Add(BuildProfileModel(principal));
                        }
                        catch (Exception ex)
                        {
                            // try again
                            profile = await _repo.FindByGlobalId(sub);
                            if (profile == null)
                            {
                                throw ex;
                            }
                        }
                    }

                    claims.Add(new Claim("role", profile.Role));
                    claims.Add(new Claim(JwtRegisteredClaimNames.NameId, profile.Id.ToString()));
                    claims.Add(new Claim("workspacelimit", profile.WorkspaceLimit.ToString()));
                    _cache.Set(sub, claims, _cacheOptions);
                }

                ((ClaimsIdentity)principal.Identity).AddClaims(claims);
                _profile = BuildProfileModel(principal);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }

            return principal;
        }
    }
}
