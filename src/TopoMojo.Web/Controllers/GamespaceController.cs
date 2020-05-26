// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    public class GamespaceController : _Controller
    {
        public GamespaceController(
            GamespaceService gamespaceService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp
        ) : base(sp)
        {
            _gamespaceService = gamespaceService;
            _pod = podService;
            _hub = hub;
        }

        private readonly IHypervisorService _pod;
        private readonly GamespaceService _gamespaceService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [HttpGet("api/gamespaces")]
        public async Task<ActionResult<Gamespace[]>> List(string filter, CancellationToken ct)
        {
            var result = await _gamespaceService.List(filter, ct);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("api/gamespace/{id}/preview")]
        public async Task<ActionResult<GameState>> Preview(int id)
        {
            var result = await _gamespaceService.LoadPreview(id);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}")]
        public async Task<ActionResult<GameState>> Load(int id)
        {
            var result = await _gamespaceService.LoadFromTopo(id);
            return Ok(result);
        }

        [HttpPost("api/gamespace/{id}/launch")]
        public async Task<ActionResult<GameState>> Launch(int id)
        {
            var result = await _gamespaceService.Launch(id);
            Log("launched", result);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}/state")]
        public async Task<ActionResult<GameState>> CheckState(int id)
        {
            var result = await _gamespaceService.Load(id);
            return Ok(result);
        }

        [HttpDelete("api/gamespace/{id}")]
        public async Task<ActionResult> Destroy(int id)
        {
            var result = await _gamespaceService.Destroy(id);

            Log("destroyed", result);

            SendBroadcast(result, "OVER");

            return Ok();
        }

        [HttpPost("api/player/enlist/{code}")]
        public async Task<ActionResult<bool>> Enlist(string code)
        {
            await _gamespaceService.Enlist(code);
            return Ok();
        }

        [HttpDelete("api/player/{id}")]
        public async Task<ActionResult<bool>> Delist(int id)
        {
            await _gamespaceService.Delist(id);
            return Ok();
        }

        [HttpGet("api/gamespace/{id}/players")]
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
