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
    [Authorize(Policy = "TrustedClients")]
    [ApiController]
    // [ApiExplorerSettings(IgnoreApi = true)]
    public class EngineController : _Controller
    {
        public EngineController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            EngineService engineService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub
        ) : base(logger, identityResolver)
        {
            _engineService = engineService;
            _pod = podService;
            _hub = hub;
        }

        private readonly IHypervisorService _pod;
        private readonly EngineService _engineService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        /// <summary>
        /// List gamespaces published for a client.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("api/engine/gamespaces")]
        public async Task<ActionResult<WorkspaceSummary>> List([FromQuery]Search search, CancellationToken ct)
        {
            var result = await _engineService.ListWorkspaces(search, ct);

            return Ok(result);
        }

        /// <summary>
        /// Create a new gamespace.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("api/engine/gamespace")]
        public async Task<ActionResult<GameState>> Launch([FromBody] GamespaceSpec model)
        {
            var result = await _engineService.Launch(model);

            Log("launched", result);

            return Ok(result);
        }

        /// <summary>
        /// Delete a gamespace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("api/engine/gamespace/{id}")]
        public async Task<ActionResult> Destroy([FromRoute]string id)
        {
            await _engineService.Destroy(id);

            Log("destroyed", new GameState {GlobalId = id});

            return Ok();
        }

        /// <summary>
        /// Request vm console access ticket.
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <returns></returns>
        [HttpGet("api/engine/vm-console/{id}")]
        public async Task<ActionResult<ConsoleSummary>> Ticket([FromRoute]string id)
        {
            return Ok(await _engineService.Ticket(id));
        }

        /// <summary>
        /// Get a workspace's templates.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/engine/templates/{id}")]
        public async Task<ActionResult<string>> Templates([FromRoute]int id)
        {
            return Ok(await _engineService.GetTemplates(id));
        }

        /// <summary>
        /// Change vm state.
        /// </summary>
        /// <param name="vmAction"></param>
        /// <returns></returns>
        [HttpPut("api/engine/vm")]
        public async Task<IActionResult> ChangeVm([FromBody] VmAction vmAction)
        {
            return Ok(await _engineService.ChangeVm(vmAction));
        }

        // /// <summary>
        // /// Show usage information.
        // /// </summary>
        // /// <returns></returns>
        // [HttpGet("api/engine")]
        // public ActionResult<string> Usage()
        // {
        //     return Ok("See API documentation.");
        // }

    }
}
