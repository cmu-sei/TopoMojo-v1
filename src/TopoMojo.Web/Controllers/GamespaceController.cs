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
        [ProducesResponseType(typeof(Gamespace[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> List()
        {
            var result = await _mgr.List();
            return Ok(result);
        }

        [HttpGet("api/gamespaces/all")]
        [ProducesResponseType(typeof(Gamespace[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> ListAll()
        {
            var result = await _mgr.ListAll();
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}")]
        [ProducesResponseType(typeof(GameState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Load([FromRoute] int id)
        {
            var result = await _mgr.LoadFromTopo(id);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}/launch")]
        [ProducesResponseType(typeof(GameState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Launch([FromRoute] int id)
        {
            var result = await _mgr.Launch(id);
            Log("launched", result);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}/state")]
        [ProducesResponseType(typeof(GameState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> CheckState([FromRoute] int id)
        {
            var result = await _mgr.Load(id);
            return Ok(result);
        }

        [HttpDelete("api/gamespace/{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Destroy([FromRoute] int id)
        {
            var result = await _mgr.Destroy(id);
            Log("destroyed", result);
            SendBroadcast(result, "OVER");
            return Ok(true);
        }

        [HttpGet("api/player/enlist/{code}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Enlist([FromRoute] string code)
        {
            return Ok(await _mgr.Enlist(code));
        }

        [HttpDelete("api/player/delist/{playerId}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delist([FromRoute] int playerId)
        {
            return Ok(await _mgr.Delist(playerId));
        }

        [HttpGet("api/gamespace/{id}/players")]
        [ProducesResponseType(typeof(Player[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Players([FromRoute] int id)
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