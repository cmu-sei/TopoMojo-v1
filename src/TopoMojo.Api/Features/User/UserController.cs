// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TopoMojo.Hubs;
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
            IHubContext<AppHub, IHubEvent> hub,
            UserService userService,
            IDistributedCache distributedCache
        ) : base(logger, hub)
        {
            _svc = userService;
            _distCache = distributedCache;
            _random = new Random();
            _cacheOpts = new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30)
            };
        }

        private readonly UserService _svc;
        private readonly IDistributedCache _distCache;
        private readonly Random _random;
        private DistributedCacheEntryOptions _cacheOpts;

        /// <summary>
        /// List users. (admin only)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("api/users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<User[]>> List([FromQuery]UserSearch model, CancellationToken ct)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin
            );

            var result = await _svc.List(model, ct);

            return Ok(result);
        }

        /// <summary>
        /// Get user profile.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/user/{id?}")]
        public async Task<ActionResult<User>> Load(string id)
        {
            id = id ?? Actor.Id;

            await Validate(new Entity{ Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => id == Actor.Id
            );

            return Ok(
                await _svc.Load(id)
            );
        }

        /// <summary>
        /// Get user's workspaces.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/user/{id}/workspaces")]
        public async Task<ActionResult<WorkspaceSummary[]>> LoadWorkspaces(string id)
        {
            await Validate(new Entity{ Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => id == Actor.Id
            );

            return Ok(
                await _svc.LoadWorkspaces(id)
            );
        }

        /// <summary>
        /// Get user's gamespaces.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/user/{id}/gamespaces")]
        public async Task<ActionResult<WorkspaceSummary[]>> LoadGamespaces(string id)
        {
            await Validate(new Entity{ Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => id == Actor.Id
            );

            return Ok(
                await _svc.LoadGamespaces(id)
            );
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("api/user")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<User>> AddOrUpdate([FromBody]User model)
        {
            AuthorizeAny(
                () => Actor.IsAdmin
            );

            return Ok(
                await _svc.AddOrUpdate(model)
            );
        }

        /// <summary>
        /// Delete user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("api/user/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await Validate(new Entity{ Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => id == Actor.Id
            );

            await _svc.Delete(id);

            return Ok();
        }

        /// <summary>
        /// Generate an ApiKey
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("api/apikey/{id}")]
        [Authorize("AdminOnly")]
        public async Task<ActionResult<ApiKeyResult>> CreateApiKey(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin
            );

            return Ok(
                await _svc.CreateApiKey(id, Actor.Name)
            );
        }

        /// <summary>
        /// Delete an ApiKey
        /// </summary>
        /// <param name="keyId"></param>
        /// <returns></returns>
        [HttpDelete("api/apikey/{id}")]
        [Authorize("AdminOnly")]
        public async Task<ActionResult> DeleteApiKey(string keyId)
        {

            AuthorizeAny(
                () => Actor.IsAdmin
            );

            await _svc.DeleteApiKey(keyId);

            return Ok();
        }

        /// <summary>
        /// Add or Update the actors user record
        /// </summary>
        /// <returns></returns>
        [HttpPost("api/user/register")]
        [Authorize]
        public async Task<User> Register()
        {
            return await _svc.AddOrUpdate(new UserRegistration
            {
                Id = Actor.Id,
                Name = Actor.Name
            });
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
        [Authorize(Policy = "Players")]
        public async Task<IActionResult> GetTicket()
        {
            string token = Guid.NewGuid().ToString("n");

            string key = $"{TicketAuthentication.TicketCachePrefix}{token}";

            string value = $"{Actor.Id}#{Actor.Name}";

            await _distCache.SetStringAsync(key, value, _cacheOpts);

            return Ok(new { Ticket = token });
        }

        /// <summary>
        /// Get auth cookie
        /// </summary>
        /// <remarks>
        /// Used to exhange one-time-ticket for an auth cookie.
        /// Also gives jwt users cookie for vm console auth.
        /// </remarks>
        /// <returns></returns>
        [HttpPost("/api/user/login")]
        [Authorize(Policy = "Players")]
        public async Task<IActionResult> GetAuthCookie()
        {
            if (User.Identity.AuthenticationType == AppConstants.CookieScheme)
                return Ok();

            await HttpContext.SignInAsync(
                AppConstants.CookieScheme,
                new ClaimsPrincipal(
                    new ClaimsIdentity(User.Claims, AppConstants.CookieScheme)
                )
            );

            return Ok();
        }

        /// <summary>
        /// End a cookie auth session
        /// </summary>
        /// <returns></returns>
        [HttpPost("/api/user/logout")]
        public async Task Logout()
        {
            if (User.Identity.AuthenticationType == AppConstants.CookieScheme)
                await HttpContext.SignOutAsync(AppConstants.CookieScheme);
        }

    }

}
