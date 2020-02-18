// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Models.Workspace;
using TopoMojo.Web;

namespace TopoMojo.Controllers
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
        [HttpGet("api/workspace/summaries")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<WorkspaceSummary>>> List(Search search)
        {
            var result = await _workspaceService.List(search);
            return Ok(result);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("api/workspaces")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<Workspace>>> ListDetail(Search search)
        {
            var result = await _workspaceService.ListDetail(search);
            return Ok(result);
        }

        [HttpPost("api/workspace")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Workspace>> Create([FromBody]NewWorkspace model)
        {
            Workspace topo = await _workspaceService.Create(model);
            return Ok(topo);
        }

        [HttpPut("api/workspace")]
        [JsonExceptionFilter]
        public async Task<ActionResult> Update([FromBody]ChangedWorkspace model)
        {
            Workspace topo = await _workspaceService.Update(model);
            // Broadcast(topo.GlobalId, new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            await _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.UPDATED", topo));
            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("api/workspace/priv")]
        [JsonExceptionFilter]
        public async Task<ActionResult> UpdatePrivilegedChanges([FromBody] PrivilegedWorkspaceChanges model)
        {
            Workspace topo = await _workspaceService.UpdatePrivilegedChanges(model);
            // Broadcast(topo.GlobalId, new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            await _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.UPDATED", topo));
            return Ok();
        }

        [HttpGet("api/workspace/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Workspace>> Load(int id)
        {
            Workspace topo = await _workspaceService.Load(id);
            return Ok(topo);
        }

        [HttpDelete("api/workspace/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            Workspace topo = await _workspaceService.Delete(id);
            Log("deleted", topo);
            await _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Workspace>(User, "TOPO.DELETED", topo));
            return Ok(true);
        }

        [HttpGet("api/workspace/{id}/games")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState[]>> LoadGames(int id)
        {
            GameState[] games = await _workspaceService.GetGames(id);
            return Ok(games);
        }

        [HttpDelete("api/workspace/{id}/games")]
        [JsonExceptionFilter]
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
        [JsonExceptionFilter]
        public async Task<ActionResult<WorkspaceState>> ChangeState(int id, [FromBody]WorkspaceStateAction action)
        {
            return Ok(await _workspaceService.ChangeState(action));
        }

        // [HttpGet("api/workspace/{id}/publish")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Publish(int id)
        // {
        //     TopologyState state = await _mgr.Publish(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/unpublish")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Unpublish(int id)
        // {
        //     TopologyState state = await _mgr.Publish(id, true);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/lock")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Lock(int id)
        // {
        //     TopologyState state = await _mgr.Lock(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/unlock")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Unlock(int id)
        // {
        //     TopologyState state = await _mgr.Lock(id, true);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/share")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Share(int id)
        // {
        //     TopologyState state = await _mgr.Share(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/workspace/{id}/unshare")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Unshare(int id)
        // {
        //     TopologyState state = await _mgr.Share(id, true);
        //     return Ok(state);
        // }

        [HttpPost("api/worker/enlist/{code}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Enlist(string code)
        {
            return Ok(await _workspaceService.Enlist(code));
        }

        [HttpDelete("api/worker/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Delist(int id)
        {
            return Ok(await _workspaceService.Delist(id));
        }

        [HttpGet("api/workspace/{id}/isos")]
        [JsonExceptionFilter]
        public async Task<ActionResult<VmOptions>> Isos(string id)
        {
            VmOptions result = await _pod.GetVmIsoOptions(id);
            return Ok(result);
        }

        [HttpGet("api/workspace/{id}/nets")]
        [JsonExceptionFilter]
        public async Task<ActionResult<VmOptions>> Nets(string id)
        {
            VmOptions result = await _pod.GetVmNetOptions(id);
            return Ok(result);
        }
    }
}
