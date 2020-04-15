// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize(Policy = "TrustedClients")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class EngineController : _Controller
    {
        public EngineController(
            EngineService engineService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp
        ) : base(sp)
        {
            _engineService = engineService;
            _pod = podService;
            _hub = hub;
        }

        private readonly IHypervisorService _pod;
        private readonly EngineService _engineService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [HttpPost("api/engine")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState>> Launch([FromBody] NewGamespace model)
        {
            var result = await _engineService.Launch(model.Workspace, model.Id);
            Log("launched", result);
            return Ok(result);
        }

        [HttpDelete("api/engine/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Destroy([FromRoute]string id)
        {
            var result = await _engineService.Destroy(id);
            Log("destroyed", result);
            return Ok(true);
        }

        [HttpGet("api/engine/ticket/{vmId}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<ConsoleSummary>> Ticket([FromRoute]string vmId)
        {
            return Ok(await _engineService.Ticket(vmId));
        }

        [HttpGet("api/engine/topo/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<string>> Templates([FromRoute]int id)
        {
            return Ok(await _engineService.GetTemplates(id));
        }

        [HttpPut("api/engine/vmaction")]
        [JsonExceptionFilter]
        public async Task<IActionResult> ChangeVm([FromBody] VmAction vmAction)
        {
            return Ok(await _engineService.ChangeVm(vmAction));
        }

        [HttpGet("api/engine")]
        [JsonExceptionFilter]
        public async Task<ActionResult<string>> Test()
        {
            await Task.Delay(0);
            return Ok("Test output.");
        }

    }
}
