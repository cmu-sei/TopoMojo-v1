// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class GamespaceController : _Controller
    {
        public GamespaceController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            GamespaceService gamespaceService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub
        ) : base(logger, identityResolver)
        {
            _gamespaceService = gamespaceService;
            _pod = podService;
            _hub = hub;
        }

        private readonly IHypervisorService _pod;
        private readonly GamespaceService _gamespaceService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

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
