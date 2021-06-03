// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
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
    // [Authorize]
    [Authorize(Policy = "Players")]
    [ApiController]
    public class GamespaceController : _Controller
    {
        public GamespaceController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            CoreOptions options,
            GamespaceService gamespaceService,
            IHypervisorService podService,
            IHubContext<AppHub, IHubEvent> hub,
            IDistributedCache cache
        ) : base(logger, identityResolver)
        {
            _gamespaceService = gamespaceService;
            _pod = podService;
            _hub = hub;
            _cache = cache;
            _options = options;
        }

        private readonly IHypervisorService _pod;
        private readonly GamespaceService _gamespaceService;
        private readonly IHubContext<AppHub, IHubEvent> _hub;
        private readonly IDistributedCache _cache;
        private readonly CoreOptions _options;

        /// <summary>
        /// List running gamespaces.
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
        /// Preview a gamespace.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Resource Id</param>
        /// <returns></returns>
        [HttpGet("api/preview/{id}")]
        public async Task<ActionResult<GameState>> Preview(string id)
        {
            var result = await _gamespaceService.Preview(id);

            return Ok(result);
        }

        /// <summary>
        /// Load a gamespace state.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpGet("api/gamespace/{id}")]
        public async Task<ActionResult<GameState>> Load(string id)
        {
            var result = await _gamespaceService.Load(id);

            return Ok(result);
        }

        /// <summary>
        /// Register a gamespace on behalf of a user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("api/gamespace")]
        public async Task<ActionResult<GameState>> Register([FromBody]RegistrationRequest model, CancellationToken ct)
        {
            var result = await _gamespaceService.Register(model);

            string token = Guid.NewGuid().ToString("N");

            await _cache.SetStringAsync(
                $"{TicketAuthentication.TicketCachePrefix}{token}",
                $"{model.SubjectId}#{model.SubjectName}",
                new DistributedCacheEntryOptions {
                    SlidingExpiration = new TimeSpan(0, 0, 60)
                },
                ct
            );

            result.LaunchpointUrl = $"{_options.LaunchUrl}?t={token}&g={result.GlobalId}";

            // if url is relative, make absolute
            if (!result.LaunchpointUrl.Contains("://"))
            {
                result.LaunchpointUrl = string.Format("{0}://{1}{2}{3}",
                    Request.Scheme,
                    Request.Host,
                    Request.PathBase,
                    result.LaunchpointUrl
                );
            }

            return Ok(result);
        }

        /// <summary>
        /// Start a gamespace.
        /// </summary>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpPost("api/gamespace/{id}/start")]
        public async Task<ActionResult<GameState>> Start(string id)
        {
            var result = await _gamespaceService.Start(id);

            return Ok(result);
        }

        /// <summary>
        /// Stop a gamespace.
        /// </summary>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpPost("api/gamespace/{id}/stop")]
        public async Task<ActionResult<GameState>> Stop(string id)
        {
            var result = await _gamespaceService.Stop(id);

            return Ok(result);
        }

        /// <summary>
        /// Complete a gamespace.
        /// </summary>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpPost("api/gamespace/{id}/complete")]
        public async Task<ActionResult<GameState>> Complete(string id)
        {
            var result = await _gamespaceService.Complete(id);

            return Ok(result);
        }

        /// <summary>
        /// Grade a challenge.
        /// </summary>
        /// <param name="id">Gamespace Id</param>
        /// <param name="model">ChallengeView</param>
        /// <returns></returns>
        [HttpPost("api/gamespace/{id}/grade")]
        public async Task<ActionResult<TopoMojo.Models.v2.ChallengeView>> Grade(string id, [FromBody]TopoMojo.Models.v2.SectionSubmission model)
        {
            var result = await _gamespaceService.Grade(id, model);

            // Log("launched", result);

            return Ok(result);
        }

        /// <summary>
        /// Delete a gamespace.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Gamespace Id</param>
        /// <returns></returns>
        [HttpDelete("api/gamespace/{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            await _gamespaceService.Delete(id);

            SendBroadcast(new GameState{GlobalId = id}, "OVER");

            return Ok();
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
