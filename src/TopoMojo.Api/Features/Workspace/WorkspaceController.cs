// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Api.Hubs;
using TopoMojo.Hypervisor;
using TopoMojo.Api.Models;
using TopoMojo.Api.Services;
using TopoMojo.Api.Validators;

namespace TopoMojo.Api.Controllers
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
        [Authorize]
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

            Workspace workspace = await _svc.Update(model, Actor.IsAdmin);

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
        [Authorize]
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
        [Authorize]
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
        [HttpGet("api/workspace/{id}/stats")]
        [Authorize]
        public async Task<ActionResult<WorkspaceStats>> GetStats(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _svc.GetStats(id)
            );
        }

        /// <summary>
        /// Load templates availabe to workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/workspace/{id}/templates")]
        [Authorize]
        public async Task<ActionResult<TemplateSummary[]>> LoadTemplates(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _svc.GetScopedTemplates(id)
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
        public async Task<ActionResult<WorkspaceStats>> DeleteGames(string id)
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

            return Ok(
                await GetStats(id)
            );
        }

        /// <summary>
        /// Generate an invitation code for worker enlistment.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpPut("api/workspace/{id}/invite")]
        [Authorize]
        public async Task<ActionResult<JoinCode>> Invite(string id)
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
        /// Get a workspace's challenge spec
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/challenge/{id}")]
        [Authorize]
        public async Task<ActionResult<ChallengeSpec>> GetChallengeSpec([FromRoute] string id)
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

        /// <summary>
        /// Update a workspace's challenge spec
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("api/challenge/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateChallengeSpec([FromRoute]string id, [FromBody] ChallengeSpec model)
        {
            await Validate(new Entity{ Id = id });

            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            await _svc.UpdateChallenge(id, model);

            // TODO: broadcast updated

            return Ok();
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
        public async Task<ActionResult<WorkspaceSummary>> Enlist(string code)
        {
            return Ok(
                await _svc.Enlist(code, Actor.Id, Actor.Name)
            );
        }

        /// <summary>
        /// Removes a worker from the workspace.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <param name="sid">Subject Id of target member</param>
        /// <returns></returns>
        [HttpDelete("api/workspace/{id}/worker/{sid}")]
        [Authorize]
        public async Task<ActionResult> Delist([FromRoute] string id, string sid)
        {
            await Validate(new Entity{ Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanManage(id, Actor.Id).Result
            );

            await _svc.Delist(id, sid, Actor.IsAdmin);

            return Ok();
        }


    }

}
