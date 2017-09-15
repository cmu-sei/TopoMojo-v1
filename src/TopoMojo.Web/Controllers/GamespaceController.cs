using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class GamespaceController : HubController<TopologyHub>
    {
        public GamespaceController(
            GamespaceManager instanceManager,
            IPodManager podManager,
            IServiceProvider sp,
            IConnectionManager sigr
        ) : base(sigr, sp)
        {
            _pod = podManager;
            _mgr = instanceManager;
        }

        private readonly IPodManager _pod;
        private readonly GamespaceManager _mgr;


        [HttpGet("api/gamespaces")]
        [ProducesResponseType(typeof(Gamespace[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> List()
        {
            var result = await _mgr.List();
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}")]
        [ProducesResponseType(typeof(GameState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Load([FromRoute]int id)
        {
            var result = await _mgr.LoadFromTopo(id);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}/launch")]
        [ProducesResponseType(typeof(GameState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Launch([FromRoute]int id)
        {
            var result = await _mgr.Launch(id);
            return Ok(result);
        }

        [HttpGet("api/gamespace/{id}/state")]
        [ProducesResponseType(typeof(GameState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> CheckState([FromRoute]int id)
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
            //TODO: Broadcast
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


    }
}