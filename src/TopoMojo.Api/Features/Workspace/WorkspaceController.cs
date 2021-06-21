// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Hubs;
using TopoMojo.Hypervisor;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class WorkspaceController : _Controller
    {
        public WorkspaceController(
            ILogger<WorkspaceController> logger,
            IHubContext<AppHub, IHubEvent> hub,
            WorkspaceValidator validator,
            IHypervisorService podService,
            WorkspaceService workspaceService
        ) : base(logger, hub, validator)
        {
            _pod = podService;
            _svc = workspaceService;
        }

        private readonly IHypervisorService _pod;
        private readonly WorkspaceService _svc;

        /// <summary>
        /// List workspaces according to search parameters.
        /// </summary>
        /// <remarks>
        /// ?aud=value retrieves item published to that audience,
        /// if the requestor is allowed that scope
        /// sort: age for newest first; default is alpha
        /// </remarks>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("api/workspaces")]
        [Authorize]
        public async Task<ActionResult<WorkspaceSummary[]>> List([FromQuery]WorkspaceSearch search, CancellationToken ct)
        {
            await Validate(search);

            await Validate(new ClientAudience
            {
                Audience = search.aud,
                Scope = Actor.Scope
            });

            AuthorizeAll();

            return Ok(
                await _svc.List(search, Actor.Id, Actor.IsAdmin, ct)
            );
        }

        /// <summary>
        /// Load a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}")]
        public async Task<ActionResult<Workspace>> Load(string id)
        {
            await Validate(new Entity{ Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _svc.Load(id)
            );
        }

        /// <summary>
        /// Create a new workspace.
        /// </summary>
        /// <param name="model">New Workspace</param>
        /// <returns>A new workspace.</returns>
        [HttpPost("api/workspace")]
        [Authorize]
        public async Task<ActionResult<Workspace>> Create([FromBody]NewWorkspace model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => Actor.IsCreator,
                () => _svc.CheckWorkspaceLimit(Actor.Id).Result
            );

            return Ok(
                await _svc.Create(model, Actor.Id)
            );
        }

        /// <summary>
        /// Create a new workspace.
        /// </summary>
        /// <param name="id">Workspace Id to clone</param>
        /// <returns>A new workspace.</returns>
        [HttpPost("api/workspace/{id}/clone")]
        [Authorize]
        public async Task<ActionResult<Workspace>> Clone([FromRoute]string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => Actor.IsCreator,
                () => _svc.CheckWorkspaceLimit(Actor.Id).Result
            );

            return Ok(
                await _svc.Clone(id)
            );
        }

        /// <summary>
        /// Update an existing workspace.
        /// </summary>
        /// <param name="model">Changed Workspace</param>
        /// <returns></returns>
        [HttpPut("api/workspace")]
        [Authorize]
        public async Task<ActionResult> Update([FromBody]ChangedWorkspace model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(model.Id, Actor.Id).Result
            );

            Workspace workspace = await _svc.Update(model);

            await Hub.Clients
                .Group(workspace.Id)
                .TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.UPDATED", workspace))
            ;

            return Ok();
        }

        /// <summary>
        /// Delete a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpDelete("api/workspace/{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanManage(id, Actor.Id).Result
            );

            var workspace = await _svc.Delete(id);

            Log("deleted", workspace);

            await Hub.Clients
                .Group(workspace.Id)
                .TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.DELETED", workspace));

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
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _pod.GetVmIsoOptions(id)
            );
        }

        /// <summary>
        /// Find virtual networks available to a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}/nets")]
        public async Task<ActionResult<VmOptions>> Nets(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _pod.GetVmNetOptions(id)
            );
        }

        /// <summary>
        /// Load gamespaces generated from a workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}/games")]
        [Authorize]
        public async Task<ActionResult<GameState[]>> LoadGames(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _svc.GetGames(id)
            );
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
        [Authorize]
        public async Task<ActionResult> DeleteGames(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            var games = await _svc.KillGames(id);

            List<Task> tasklist = new List<Task>();

            foreach (var game in games)
                tasklist.Add(
                    Hub.Clients
                        .Group(game.Id)
                        .GameEvent(new BroadcastEvent<GameState>(User, "GAME.OVER", game))
                );

            await Task.WhenAll(tasklist.ToArray());

            return Ok();
        }

        /// <summary>
        /// Generate an invitation code for worker enlistment.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpPut("api/workspace/{id}/invite")]
        [Authorize]
        public async Task<ActionResult<WorkspaceInvitation>> Invite(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanManage(id, Actor.Id).Result
            );

            return Ok(
                await _svc.Invite(id)
            );
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
        [Authorize]
        public async Task<ActionResult> Enlist(string code)
        {
            return Ok(
                await _svc.Enlist(code, Actor.Id, Actor.Name)
            );
        }

        /// <summary>
        /// Removes a worker from the workspace.
        /// </summary>
        /// <param name="model">Worker</param>
        /// <returns></returns>
        [HttpDelete("api/worker")]
        public async Task<ActionResult> Delist([FromBody] Worker model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanManage(model.WorkspaceId, Actor.Id).Result
            );

            await _svc.Delist(model.WorkspaceId, Actor.Id, Actor.IsAdmin);

            return Ok();
        }

        [HttpGet("api/workspace/{id}/challenge")]
        [Authorize]
        public async Task<IActionResult> GetChallenge([FromRoute] string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _svc.GetChallenge(id)
            );
        }

        [HttpPut("api/workspace/{id}/challenge")]
        [Authorize]
        public async Task<IActionResult> ChallengeV2([FromRoute]string id, [FromBody] ChallengeSpec model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            await _svc.UpdateChallenge(id, model);

            // TODO: broadcast updated

            return Ok();
        }
    }

}
