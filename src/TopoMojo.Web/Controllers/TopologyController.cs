using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class TopologyController : _Controller
    {
        public TopologyController(
            TopologyManager topologyManager,
            IPodManager podManager,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp
        ) : base(sp)
        {
            _pod = podManager;
            _mgr = topologyManager;
            _hub = hub;
        }

        private readonly IPodManager _pod;
        private readonly TopologyManager _mgr;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [AllowAnonymous]
        [HttpGet("api/topology/summaries")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<TopologySummary>>> List(Search search)
        {
            var result = await _mgr.List(search);
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("api/topologies")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<Topology>>> ListDetail(Search search)
        {
            var result = await _mgr.ListDetail(search);
            return Ok(result);
        }

        [HttpPost("api/topology")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Topology>> Create([FromBody]NewTopology model)
        {
            Topology topo = await _mgr.Create(model);
            return Ok(topo);
        }

        [HttpPut("api/topology")]
        [JsonExceptionFilter]
        public async Task<ActionResult> Update([FromBody]ChangedTopology model)
        {
            Topology topo = await _mgr.Update(model);
            // Broadcast(topo.GlobalId, new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            await _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            return Ok();
        }

        [HttpGet("api/topology/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Topology>> Load(int id)
        {
            Topology topo = await _mgr.Load(id);
            return Ok(topo);
        }

        [HttpDelete("api/topology/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            Topology topo = await _mgr.Delete(id);
            Log("deleted", topo);
            await _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Topology>(User, "TOPO.DELETED", topo));
            return Ok(true);
        }

        [HttpGet("api/topology/{id}/games")]
        [JsonExceptionFilter]
        public async Task<ActionResult<GameState[]>> LoadGames(int id)
        {
            GameState[] games = await _mgr.GetGames(id);
            return Ok(games);
        }

        [HttpDelete("api/topology/{id}/games")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> DeleteGames(int id)
        {
            var games = await _mgr.KillGames(id);
            List<Task> tasklist = new List<Task>();
            foreach (var game in games)
                tasklist.Add(_hub.Clients.Group(game.GlobalId).GameEvent(new BroadcastEvent<GameState>(User, "GAME.OVER", game)));
            Task.WaitAll(tasklist.ToArray());
            return Ok(true);
        }

        [Obsolete]
        [HttpPost("api/topology/{id}/action")]
        [JsonExceptionFilter]
        public async Task<ActionResult<TopologyState>> ChangeState(int id, [FromBody]TopologyStateAction action)
        {
            return Ok(await _mgr.ChangeState(action));
        }

        // [HttpGet("api/topology/{id}/publish")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Publish(int id)
        // {
        //     TopologyState state = await _mgr.Publish(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/topology/{id}/unpublish")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Unpublish(int id)
        // {
        //     TopologyState state = await _mgr.Publish(id, true);
        //     return Ok(state);
        // }

        // [HttpGet("api/topology/{id}/lock")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Lock(int id)
        // {
        //     TopologyState state = await _mgr.Lock(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/topology/{id}/unlock")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Unlock(int id)
        // {
        //     TopologyState state = await _mgr.Lock(id, true);
        //     return Ok(state);
        // }

        // [HttpGet("api/topology/{id}/share")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<TopologyState>> Share(int id)
        // {
        //     TopologyState state = await _mgr.Share(id, false);
        //     return Ok(state);
        // }

        // [HttpGet("api/topology/{id}/unshare")]
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
            return Ok(await _mgr.Enlist(code));
        }

        [HttpDelete("api/worker/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Delist(int id)
        {
            return Ok(await _mgr.Delist(id));
        }

        [HttpGet("api/topology/{id}/isos")]
        [JsonExceptionFilter]
        public async Task<ActionResult<VmOptions>> Isos(string id)
        {
            VmOptions result = await _pod.GetVmIsoOptions(id);
            return Ok(result);
        }

        [HttpGet("api/topology/{id}/nets")]
        [JsonExceptionFilter]
        public async Task<ActionResult<VmOptions>> Nets(string id)
        {
            VmOptions result = await _pod.GetVmNetOptions(id);
            return Ok(result);
        }
    }
}
