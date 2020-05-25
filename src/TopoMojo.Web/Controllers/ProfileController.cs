// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    public class ProfileController : _Controller
    {
        public ProfileController(
            UserService userService,
            IIdentityResolver identityResolver,
            IServiceProvider sp,
            IDataProtectionProvider dp
        ) : base(sp)
        {
            _userService = userService;
            _identity = identityResolver;
            _dp = dp.CreateProtector($"_dp:{Assembly.GetEntryAssembly().FullName}");
        }

        private readonly UserService _userService;
        private readonly IIdentityResolver _identity;
        private readonly IDataProtector _dp;

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("api/profiles")]
        public async Task<ActionResult<SearchResult<User>>> List(Search search)
        {
            var result = await _userService.List(search);

            return Ok(result);
        }

        [HttpGet("api/profile")]
        public async Task<ActionResult<User>> GetProfile()
        {
            var result = await _userService.Load("");

            return Ok(result);
        }

        [HttpPut("api/profile")]
        public async Task<IActionResult> Update([FromBody]User model)
        {
            if (!_user.IsAdmin && model.GlobalId != _user.GlobalId)
                return Forbid();

            await _userService.Update(model);

            return Ok();
        }

        [HttpDelete("api/profile/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.Delete(id);

            return Ok();
        }

        [HttpGet("/api/profile/ticket")]
        public IActionResult GetTicket()
        {
            string ticket = $"{DateTime.UtcNow.AddSeconds(20).Ticks}|{Guid.NewGuid().ToString("N")}|{User.FindFirstValue("sub")}";

            return Ok(new { Ticket = _dp.Protect(ticket)});
        }
    }

}
