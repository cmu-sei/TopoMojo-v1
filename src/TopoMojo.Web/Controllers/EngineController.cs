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

        [HttpGet("api/engine/topo")]
        public async Task<ActionResult<WorkspaceSummary>> List(Search search, CancellationToken ct)
        {
            var result = await _engineService.ListWorkspaces(search, ct);

            return Ok(result);
        }

        [HttpPost("api/engine")]
        public async Task<ActionResult<GameState>> Launch([FromBody] NewGamespace model)
        {
            var result = await _engineService.Launch(model.Workspace, model.Id);

            Log("launched", result);

            return Ok(result);
        }

        [HttpDelete("api/engine/{id}")]
        public async Task<ActionResult> Destroy([FromRoute]string id)
        {
            await _engineService.Destroy(id);

            Log("destroyed", new GameState {GlobalId = id});

            return Ok();
        }

        [HttpGet("api/engine/ticket/{vmId}")]
        public async Task<ActionResult<ConsoleSummary>> Ticket([FromRoute]string vmId)
        {
            return Ok(await _engineService.Ticket(vmId));
        }

        [HttpGet("api/engine/topo/{id}")]
        public async Task<ActionResult<string>> Templates([FromRoute]int id)
        {
            return Ok(await _engineService.GetTemplates(id));
        }

        [HttpPut("api/engine/vmaction")]
        public async Task<IActionResult> ChangeVm([FromBody] VmAction vmAction)
        {
            return Ok(await _engineService.ChangeVm(vmAction));
        }

        [HttpGet("api/engine")]
        public ActionResult<string> Usage()
        {
            return Ok("See API documentation.");
        }

    }
}
