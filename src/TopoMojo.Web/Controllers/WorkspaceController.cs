// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Services;
using TopoMojo.Models;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class WorkspaceController : _Controller
    {
        public WorkspaceController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            WorkspaceService workspaceService,
            IHypervisorService podService,
            IHubContext<AppHub, IHubEvent> hub
        ) : base(logger, identityResolver)
        {
            _pod = podService;
            _workspaceService = workspaceService;
            _hub = hub;
        }

        private readonly IHypervisorService _pod;
        private readonly WorkspaceService _workspaceService;
        private readonly IHubContext<AppHub, IHubEvent> _hub;

        /// <summary>
        /// List workspaces according to search parameters.
        /// </summary>
        /// <remarks>
        /// filter: public, private
        /// sort: age
        /// </remarks>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("api/workspaces")]
        public async Task<ActionResult<WorkspaceSummary[]>> List([FromQuery]Search search, CancellationToken ct)
        {
            var result = await _workspaceService.List(search, ct);

            return Ok(result);
        }

        /// <summary>
        /// Load a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}")]
        public async Task<ActionResult<Workspace>> Load(int id)
        {
            Workspace workspace = await _workspaceService.Load(id);

            return Ok(workspace);
        }
        /// <summary>
        /// Load a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/v2/workspace/{id}")]
        public async Task<ActionResult<Workspace>> Load(string id)
        {
            Workspace workspace = await _workspaceService.Load(id);

            return Ok(workspace);
        }

        /// <summary>
        /// Create a new workspace.
        /// </summary>
        /// <param name="model">New Workspace</param>
        /// <returns>A new workspace.</returns>
        [HttpPost("api/workspace")]
        public async Task<ActionResult<Workspace>> Create([FromBody]NewWorkspace model)
        {
            Workspace workspace = await _workspaceService.Create(model);

            return Created(Url.Action("Load", new {Id = workspace.Id}), workspace);
        }

        /// <summary>
        /// Update an existing workspace.
        /// </summary>
        /// <param name="model">Changed Workspace</param>
        /// <returns></returns>
        [HttpPut("api/workspace")]
        public async Task<ActionResult> Update([FromBody]ChangedWorkspace model)
        {
            Workspace workspace = await _workspaceService.Update(model);

            // Broadcast(topo.GlobalId, new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            await _hub.Clients.Group(workspace.GlobalId).TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.UPDATED", workspace));

            return Ok();
        }

        /// <summary>
        /// Delete a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpDelete("api/workspace/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var workspace = await _workspaceService.Delete(id);

            Log("deleted", workspace);

            await _hub.Clients.Group(workspace.GlobalId).TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.DELETED", workspace));

            return Ok();
        }

        /// <summary>
        /// Find ISO files available to a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}/isos")]
        public async Task<ActionResult<VmOptions>> Isos(string id)
        {
            VmOptions result = await _pod.GetVmIsoOptions(id);

            return Ok(result);
        }

        /// <summary>
        /// Find virtual networks available to a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}/nets")]
        public async Task<ActionResult<VmOptions>> Nets(string id)
        {
            VmOptions result = await _pod.GetVmNetOptions(id);

            return Ok(result);
        }

        /// <summary>
        /// Load gamespaces generated from a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}/games")]
        public async Task<ActionResult<GameState[]>> LoadGames(int id)
        {
            GameState[] games = await _workspaceService.GetGames(id);

            return Ok(games);
        }

        /// <summary>
        /// Delete all gamespaces generated from this workspace.
        /// </summary>
        /// <remarks>
        /// Useful if updating a workspace after it is published.
        /// Workspace updates are disallowed if gamespaces exist.
        /// </remarks>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpDelete("api/workspace/{id}/games")]
        public async Task<ActionResult> DeleteGames(int id)
        {
            var games = await _workspaceService.KillGames(id);

            List<Task> tasklist = new List<Task>();

            foreach (var game in games)
                tasklist.Add(_hub.Clients.Group(game.GlobalId).GameEvent(new BroadcastEvent<GameState>(User, "GAME.OVER", game)));

            await Task.WhenAll(tasklist.ToArray());

            return Ok();
        }

        /// <summary>
        /// Generate an invitation code for worker enlistment.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpPut("api/workspace/{id}/invite")]
        public async Task<ActionResult<WorkspaceInvitation>> Invite(int id)
        {
            var state = await _workspaceService.Invite(id);

            return Ok(state);
        }

        /// <summary>
        /// Accept an invitation to a workspace.
        /// </summary>
        /// <remarks>
        /// Any user that submits the invitation code is
        /// added as member of the workspace.
        /// </remarks>
        /// <param name="code">Invitation Code</param>
        /// <returns></returns>
        [HttpPost("api/worker/{code}")]
        public async Task<ActionResult> Enlist(string code)
        {
            return Ok(
                await _workspaceService.Enlist(code)
            );
        }

        /// <summary>
        /// Removes a worker from the workspace.
        /// </summary>
        /// <param name="id">Worker Id</param>
        /// <returns></returns>
        [HttpDelete("api/worker/{id}")]
        public async Task<ActionResult> Delist(int id)
        {
            await _workspaceService.Delist(id);

            return Ok();
        }

        [HttpPut("api/workspace/{id}/challenge")]
        public async Task<IActionResult> Challenge([FromRoute]int id, [FromBody]ChallengeSpec model)
        {
            await _workspaceService.UpdateChallenge(id, model);

            return Ok();
        }
    }

}
