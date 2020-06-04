// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class UserController : _Controller
    {
        public UserController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            UserService userService,
            IDataProtectionProvider dp
        ) : base(logger, identityResolver)
        {
            _userService = userService;
            _identity = identityResolver;
            _dp = dp.CreateProtector($"_dp:{Assembly.GetEntryAssembly().FullName}");
        }

        private readonly UserService _userService;
        private readonly IIdentityResolver _identity;
        private readonly IDataProtector _dp;

        /// <summary>
        /// List users. (admin only)
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("api/users")]
        public async Task<ActionResult<User[]>> List([FromQuery]Search search, CancellationToken ct)
        {
            var result = await _userService.List(search, ct);

            return Ok(result);
        }

        /// <summary>
        /// Get user profile.
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/user")]
        public async Task<ActionResult<User>> GetProfile()
        {
            var result = await _userService.Load("");

            return Ok(result);
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("api/user")]
        public async Task<IActionResult> Update([FromBody]User model)
        {
            if (!_user.IsAdmin && model.GlobalId != _user.GlobalId)
                return Forbid();

            await _userService.Update(model);

            return Ok();
        }

        /// <summary>
        /// Delete user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("api/user/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_user.IsAdmin && _user.Id != id)
                return Forbid();

            await _userService.Delete(id);

            return Ok();
        }

        /// <summary>
        /// Get one-time auth ticket.
        /// </summary>
        /// <remarks>
        /// Client websocket connections can be authenticated with this ticket
        /// in an `Authorization: Ticket [ticket]` or `Authorization: Bearer [ticket]` header.
        /// </remarks>
        /// <returns></returns>
        [HttpGet("/api/user/ticket")]
        public IActionResult GetTicket()
        {
            string ticket = $"{DateTime.UtcNow.AddSeconds(20).Ticks}|{Guid.NewGuid().ToString("N")}|{User.FindFirstValue("sub")}";

            return Ok(new { Ticket = _dp.Protect(ticket)});
        }

    }

}
