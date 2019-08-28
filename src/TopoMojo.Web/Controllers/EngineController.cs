// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    public class EngineController : _Controller
    {
        public EngineController(
            EngineService instanceManager,
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
        private readonly EngineService _mgr;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [HttpPost("api/engine")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState>> Launch([FromBody] NewGamespace model)
        {
            var result = await _mgr.Launch(model.WorkspaceId, model.Id);
            Log("launched", result);
            return Ok(result);
        }

        [HttpDelete("api/engine/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Destroy(string id)
        {
            var result = await _mgr.Destroy(id);
            Log("destroyed", result);
            return Ok(true);
        }

        [HttpGet("api/engine/ticket/{vmId}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Models.Virtual.DisplayInfo>> Ticket(string vmId)
        {
            return Ok(await _mgr.Ticket(vmId));
        }

        [HttpGet("api/engine")]
        [JsonExceptionFilter]
        public async Task<ActionResult<string>> Test()
        {
            return Ok("Test output.");
        }

        public class NewGamespace
        {
            public string Id { get; set; }
            public int WorkspaceId { get; set; }
        }
    }
}
