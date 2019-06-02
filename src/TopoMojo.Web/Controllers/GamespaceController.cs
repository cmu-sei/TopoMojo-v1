// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class GamespaceController : _Controller
    {
        public GamespaceController(
            GamespaceManager instanceManager,
            IPodManager podManager,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp
        ) : base(sp)
        {
            _pod = podManager;
            _mgr = instanceManager;
            _hub = hub;
        }

        private readonly IPodManager _pod;
        private readonly GamespaceManager _mgr;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [HttpGet("api/gamespaces")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Gamespace[]>> List(string filter)
        {
            var result = await _mgr.List(filter);
            return Ok(result);
        }

        // [Authorize(Roles = "admin")]
        // [HttpGet("api/gamespaces/all")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<Gamespace[]>> ListAll()
        // {
        //     var result = await _mgr.ListAll();
        //     return Ok(result);
        // }
        [AllowAnonymous]
        [HttpGet("api/gamespace/{id}/preview")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState>> Preview(int id)
        {
            var result = await _mgr.LoadPreview(id);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState>> Load(int id)
        {
            var result = await _mgr.LoadFromTopo(id);
            return Ok(result);
        }

        [HttpPost("api/gamespace/{id}/launch")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState>> Launch(int id)
        {
            var result = await _mgr.Launch(id);
            Log("launched", result);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}/state")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState>> CheckState(int id)
        {
            var result = await _mgr.Load(id);
            return Ok(result);
        }

        [HttpDelete("api/gamespace/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Destroy(int id)
        {
            var result = await _mgr.Destroy(id);
            Log("destroyed", result);
            SendBroadcast(result, "OVER");
            return Ok(true);
        }

        [HttpPost("api/player/enlist/{code}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Enlist(string code)
        {
            return Ok(await _mgr.Enlist(code));
        }

        [HttpDelete("api/player/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Delist(int id)
        {
            return Ok(await _mgr.Delist(id));
        }

        [HttpGet("api/gamespace/{id}/players")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Player[]>> Players(int id)
        {
            return Ok(await _mgr.Players(id));
        }

        private void SendBroadcast(GameState gameState, string action)
        {
            _hub.Clients.Group(gameState.GlobalId)
                .GameEvent(new BroadcastEvent<Core.Models.GameState>(
                    User,
                    "GAME." + action.ToUpper(),
                    gameState
                ));
        }
    }
}
