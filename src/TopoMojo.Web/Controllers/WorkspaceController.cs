// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
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

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    public class WorkspaceController : _Controller
    {
        public WorkspaceController(
            WorkspaceService workspaceService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp
        ) : base(sp)
        {
            _pod = podService;
            _workspaceService = workspaceService;
            _hub = hub;
        }

        private readonly IHypervisorService _pod;
        private readonly WorkspaceService _workspaceService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [AllowAnonymous]
        [HttpGet("api/workspaces/summary")]
        public async Task<ActionResult<WorkspaceSummary>> List(Search search)
        {
            var result = await _workspaceService.List(search);

            return Ok(result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("api/workspaces")]
        public async Task<ActionResult<Workspace>> ListDetail(Search search)
        {
            var result = await _workspaceService.ListDetail(search);

            return Ok(result);
        }

        [HttpPost("api/workspace")]
        public async Task<ActionResult<Workspace>> Create([FromBody]NewWorkspace model)
        {
            Workspace topo = await _workspaceService.Create(model);

            return Ok(topo);
        }

        [HttpPut("api/workspace")]
        public async Task<ActionResult> Update([FromBody]ChangedWorkspace model)
        {
            Workspace topo = await _workspaceService.Update(model);

            // Broadcast(topo.GlobalId, new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            await _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.UPDATED", topo));

            return Ok();
        }

        [HttpGet("api/workspace/{id}")]
        public async Task<ActionResult<Workspace>> Load(int id)
        {
            Workspace topo = await _workspaceService.Load(id);
            return Ok(topo);
        }

        [HttpDelete("api/workspace/{id}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var topo = await _workspaceService.Delete(id);

            Log("deleted", topo);

            await _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.DELETED", topo));

            return Ok(true);
        }

        [HttpGet("api/workspace/{id}/games")]
        public async Task<ActionResult<GameState[]>> LoadGames(int id)
        {
            GameState[] games = await _workspaceService.GetGames(id);
            return Ok(games);
        }

        [HttpDelete("api/workspace/{id}/games")]
        public async Task<ActionResult<bool>> DeleteGames(int id)
        {
            var games = await _workspaceService.KillGames(id);

            List<Task> tasklist = new List<Task>();

            foreach (var game in games)
                tasklist.Add(_hub.Clients.Group(game.GlobalId).GameEvent(new BroadcastEvent<GameState>(User, "GAME.OVER", game)));

            Task.WaitAll(tasklist.ToArray());

            return Ok(true);
        }

        [Obsolete]
        [HttpPost("api/workspace/{id}/action")]
        public async Task<ActionResult<WorkspaceState>> ChangeState(int id, [FromBody]WorkspaceStateAction action)
        {
            return Ok(await _workspaceService.ChangeState(action));
        }

        // [HttpGet("api/workspace/{id}/publish")]
        //
        // public async Task<ActionResult<TopologyState>> Publish(int id)
        // {
        //     TopologyState state = await _mgr.Publish(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/unpublish")]
        //
        // public async Task<ActionResult<TopologyState>> Unpublish(int id)
        // {
        //     TopologyState state = await _mgr.Publish(id, true);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/lock")]
        //
        // public async Task<ActionResult<TopologyState>> Lock(int id)
        // {
        //     TopologyState state = await _mgr.Lock(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/unlock")]
        //
        // public async Task<ActionResult<TopologyState>> Unlock(int id)
        // {
        //     TopologyState state = await _mgr.Lock(id, true);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/share")]
        //
        // public async Task<ActionResult<TopologyState>> Share(int id)
        // {
        //     TopologyState state = await _mgr.Share(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/unshare")]
        //
        // public async Task<ActionResult<TopologyState>> Unshare(int id)
        // {
        //     TopologyState state = await _mgr.Share(id, true);
        //     return Ok(state);
        // }

        [HttpPost("api/worker/enlist/{code}")]
        public async Task<ActionResult> Enlist(string code)
        {
            await _workspaceService.Enlist(code);

            return Ok();
        }

        [HttpDelete("api/worker/{id}")]
        public async Task<ActionResult> Delist(int id)
        {
            await _workspaceService.Delist(id);

            return Ok();
        }

        [HttpGet("api/workspace/{id}/isos")]
        public async Task<ActionResult<VmOptions>> Isos(string id)
        {
            VmOptions result = await _pod.GetVmIsoOptions(id);

            return Ok(result);
        }

        [HttpGet("api/workspace/{id}/nets")]
        public async Task<ActionResult<VmOptions>> Nets(string id)
        {
            VmOptions result = await _pod.GetVmNetOptions(id);

            return Ok(result);
        }
    }
}
