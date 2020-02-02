// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Core;
using TopoMojo.Abstractions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    //Transforms ClaimsPrincipal to TopoMojo.Core.Models.Profile
    //(Once per request, if AddScoped())
    public class ProfileResolver : IProfileResolver, IClaimsTransformation
    {
        public ProfileResolver(
            IHttpContextAccessor context,
            IMemoryCache cache,
            PrivilegedUserService userSvc,
            ControlOptions options
        ){
            _context = context;
            _cache = cache;
            _userService = userSvc;
            _cacheOptions = new MemoryCacheEntryOptions();
            _cacheOptions.SetAbsoluteExpiration(
                TimeSpan.FromSeconds(options.ProfileCacheSeconds)
            );
        }

        private readonly IHttpContextAccessor _context;
        private readonly IMemoryCache _cache;
        private readonly PrivilegedUserService _userService;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private User _user = null;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);

        public User User {
            get
            {
                if (_user == null)
                    _user = BuildProfileModel(_context.HttpContext.User);
                return _user;
            }
        }

        private User BuildProfileModel(ClaimsPrincipal principal)
        {
            var profile = new User();
            profile.GlobalId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            profile.Name = principal.FindFirst("name")?.Value ?? "Anonymous";
            profile.Role = principal.FindFirst("role")?.Value ?? "User";
            profile.IsAdmin = principal.IsInRole("Administrator");

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
            if (_user != null)
                return principal;

            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released
            await semaphoreSlim.WaitAsync();
            try
            {
                string sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (!_cache.TryGetValue(sub, out List<Claim> claims))
                {

                    claims = new List<Claim>();
                    var profile = await _userService.FindByGlobalId(sub);
                    if (profile != null)
                    {
                        string name = principal.FindFirstValue("name");
                        if (!String.IsNullOrEmpty(name) && name != profile.Name)
                        {
                            profile.Name = name;
                            await _userService.Update(profile);
                        }
                    }
                    else
                    {
                        try
                        {
                            profile = await _userService.Add(BuildProfileModel(principal));
                        }
                        catch (Exception ex)
                        {
                            // try again
                            profile = await _userService.FindByGlobalId(sub);
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
                _user = BuildProfileModel(principal);
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
