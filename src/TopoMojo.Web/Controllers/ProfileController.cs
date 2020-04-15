// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class ProfileController : _Controller
    {
        public ProfileController(
            UserService userService,
            IIdentityResolver identityResolver,
            IServiceProvider sp,
            IMemoryCache cache
        ) : base(sp)
        {
            _userService = userService;
            _identity = identityResolver;
            _cache = cache;
        }

        private readonly UserService _userService;
        private readonly IIdentityResolver _identity;
        private readonly IMemoryCache _cache;

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("api/profiles")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<User>>> List(Search search)
        {
            var result = await _userService.List(search);
            return Ok(result);
        }

        [HttpGet("api/profile")]
        [JsonExceptionFilter]
        public async Task<ActionResult<User>> GetProfile()
        {
            var result = await _userService.FindByGlobalId("");
            return Ok(result);
        }

        [HttpPut("api/profile")]
        [JsonExceptionFilter]
        public async Task<IActionResult> UpdateProfile([FromBody]ChangedUser model)
        {
            model.GlobalId = User.FindFirstValue("sub");
            await _userService.UpdateProfile(model);
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("api/profile/priv")]
        [JsonExceptionFilter]
        public async Task<IActionResult> PrivilegedUpdate([FromBody]User profile)
        {
            await _userService.PrivilegedUpdate(profile);
            return Ok();

        }

        [HttpDelete("api/profile/{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            await _userService.DeleteProfile(id);
            return Ok();
        }

        [HttpGet("/api/profile/ticket")]
        [JsonExceptionFilter]
        public async Task<IActionResult> GetTicket()
        {
            await Task.Delay(0);
            string ticket = Guid.NewGuid().ToString("N");

            _cache.Set(
                ticket,
                User.FindFirstValue("sub"),
                new MemoryCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = new TimeSpan(0,0,20)
                }
            );

            return Ok(new { Ticket = ticket});
        }
    }

}
