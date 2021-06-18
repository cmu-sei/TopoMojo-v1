// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Hubs;
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
            IIdentityResolver identityResolver,
            WorkspaceService workspaceService,
            WorkspaceValidator validator,
            IHypervisorService podService,
            IHubContext<AppHub, IHubEvent> hub
        ) : base(logger, identityResolver, validator)
        {
            _pod = podService;
            _svc = workspaceService;

            _hub = hub;
        }

        private readonly IHypervisorService _pod;
        private readonly WorkspaceService _svc;
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
        [Authorize]
        [HttpGet("api/workspaces")]
        public async Task<ActionResult<WorkspaceSummary[]>> List([FromQuery]WorkspaceSearch search, CancellationToken ct)
        {
            await Validate(search);

            if (Actor.IsAgent)
                await Validate(new ClientAudience
                {
                    Scope = Actor.Client?.Scope,
                    Audience = search.aud
                });

            AuthorizeAll();

            return Ok(
                await _svc.List(search, Actor, ct)
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
                () => _svc.CanEdit(id, Actor).Result
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
                () => _svc.CheckWorkspaceLimit(Actor.GlobalId).Result
            );

            Workspace workspace = await _svc.Create(model, Actor.Id);

            return Ok(
                await _svc.Create(model, Actor.Id)
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
                () => _svc.CanEdit(model.GlobalId, Actor).Result
            );

            Workspace workspace = await _svc.Update(model);

            await _hub.Clients
                .Group(workspace.GlobalId)
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
                () => _svc.CanManage(id, Actor).Result
            );

            var workspace = await _svc.Delete(id);

            Log("deleted", workspace);

            await _hub.Clients
                .Group(workspace.GlobalId)
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
                () => _svc.CanEdit(id, Actor).Result
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
                () => _svc.CanEdit(id, Actor).Result
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
                () => _svc.CanEdit(id, Actor).Result
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
                () => _svc.CanEdit(id, Actor).Result
            );

            var games = await _svc.KillGames(id);

            List<Task> tasklist = new List<Task>();

            foreach (var game in games)
                tasklist.Add(
                    _hub.Clients
                        .Group(game.GlobalId)
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
                () => _svc.CanManage(id, Actor).Result
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
                await _svc.Enlist(code, Actor)
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
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanManage(id, Actor).Result
            );

            await _svc.Delist(id, Actor);

            return Ok();
        }

        [HttpGet("api/workspace/{id}/challenge")]
        [Authorize]
        public async Task<IActionResult> GetChallenge([FromRoute] string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor).Result
            );

            return Ok(
                await _svc.GetChallenge(id)
            );
        }

        [HttpPut("api/workspace/{id}/challenge")]
        [Authorize]
        public async Task<IActionResult> ChallengeV2([FromRoute]string id, [FromBody] TopoMojo.Models.v2.ChallengeSpec model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor).Result
            );

            await _svc.UpdateChallenge(id, model);

            // TODO: broadcast updated

            return Ok();
        }
    }

}
