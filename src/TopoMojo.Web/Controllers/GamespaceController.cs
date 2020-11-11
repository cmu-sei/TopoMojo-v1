// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize(Policy="Players")]
    [ApiController]
    public class GamespaceController : _Controller
    {
        public GamespaceController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            GamespaceService gamespaceService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IDistributedCache cache
        ) : base(logger, identityResolver)
        {
            _gamespaceService = gamespaceService;
            _pod = podService;
            _hub = hub;
            _cache = cache;
        }

        private readonly IHypervisorService _pod;
        private readonly GamespaceService _gamespaceService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// List user's running gamespaces.
        /// </summary>
        /// <remarks>
        /// By default, result is filtered to user's gamespaces.
        /// An administrator can override default filter with filter = "all".
        /// </remarks>
        /// <param name="filter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("api/gamespaces")]
        public async Task<ActionResult<Gamespace[]>> List(string filter, CancellationToken ct)
        {
            var result = await _gamespaceService.List(filter, ct);

            return Ok(result);
        }

        /// <summary>
        /// Load a gamespace.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("api/gamespace/{id}")]
        public async Task<ActionResult<GameState>> Load(int id)
        {
            var result = await _gamespaceService.LoadFromWorkspace(id);

            result.WorkspaceDocument = ApplyPathBase(result.WorkspaceDocument);

            return Ok(result);
        }

        /// <summary>
        /// Start a gamespace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpPost("api/gamespace/{id}")]
        public async Task<ActionResult<GameState>> Launch(int id)
        {
            var result = await _gamespaceService.Launch(id);

            result.WorkspaceDocument = ApplyPathBase(result.WorkspaceDocument);

            Log("launched", result);

            return Ok(result);
        }

        /// <summary>
        /// Start a gamespace with a registration token.
        /// </summary>
        /// <param name="token">registration token</param>
        /// <returns></returns>
        [HttpPost("api/launch/{token}")]
        public async Task<GameState> LaunchRegistered(string token)
        {
            string data = await _cache.GetStringAsync($"{AppConstants.RegistrationCachePrefix}{token}");

            if (string.IsNullOrEmpty(data))
                throw new GamespaceNotRegistered();

            try
            {

                var registration = JsonSerializer.Deserialize<Registration>(data);

                var result = await _gamespaceService.Launch(registration);

                result.WorkspaceDocument = ApplyPathBase(result.WorkspaceDocument);

                Log("launched", result);

                return result;

            }
            catch (Exception ex)
            {

                throw new GamespaceNotRegistered(ex.Message);

            }

        }

        /// <summary>
        /// End a registered gamespace.
        /// </summary>
        /// <param name="token">Registration Token</param>
        /// <returns></returns>
        [HttpDelete("api/launch/{token}")]
        public async Task<ActionResult> Destroy(string token)
        {
            string data = await _cache.GetStringAsync($"{AppConstants.RegistrationCachePrefix}{token}");

            if (string.IsNullOrEmpty(data))
                return Ok();

            var registration = JsonSerializer.Deserialize<Registration>(data);

            var result = await _gamespaceService.Destroy(registration.GamespaceId);

            if (result != null)
            {
                Log("destroyed", result);

                SendBroadcast(result, "OVER");
            }

            await _cache.RemoveAsync($"{AppConstants.RegistrationCachePrefix}{token}");

            return Ok();
        }

        /// <summary>
        /// End a gamespace.
        /// </summary>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpDelete("api/gamespace/{id}")]
        public async Task<ActionResult> Destroy(int id)
        {
            var result = await _gamespaceService.Destroy(id);

            Log("destroyed", result);

            SendBroadcast(result, "OVER");

            return Ok();
        }

        /// <summary>
        /// Get current game state.
        /// </summary>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpGet("api/gamestate/{id}")]
        public async Task<ActionResult<GameState>> CheckState(int id)
        {
            var result = await _gamespaceService.Load(id);

            result.WorkspaceDocument = ApplyPathBase(result.WorkspaceDocument);

            return Ok(result);
        }

        /// <summary>
        /// Accept an invitation to a gamespace.
        /// </summary>
        /// <param name="code">Invitation Code</param>
        /// <returns></returns>
        [HttpPost("api/player/{code}")]
        public async Task<ActionResult<bool>> Enlist(string code)
        {
            await _gamespaceService.Enlist(code);

            return Ok();
        }

        /// <summary>
        /// Remove a player from a gamespace.
        /// </summary>
        /// <param name="id">Player Id</param>
        /// <returns></returns>
        [HttpDelete("api/player/{id}")]
        public async Task<ActionResult<bool>> Delist(int id)
        {
            await _gamespaceService.Delist(id);

            return Ok();
        }

        /// <summary>
        /// List gamespace players.
        /// </summary>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpGet("api/players/{id}")]
        public async Task<ActionResult<Player[]>> Players(int id)
        {
            return Ok(await _gamespaceService.Players(id));
        }

        private void SendBroadcast(GameState gameState, string action)
        {
            _hub.Clients.Group(gameState.GlobalId)
                .GameEvent(new BroadcastEvent<GameState>(
                    User,
                    "GAME." + action.ToUpper(),
                    gameState
                ));
        }

    }
}
