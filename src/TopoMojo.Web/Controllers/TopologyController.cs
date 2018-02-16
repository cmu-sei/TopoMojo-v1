using System;
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
    [Authorize(AuthenticationSchemes = "IdSrv,Bearer")]
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
        [HttpGet("api/topologies")]
        [ProducesResponseType(typeof(SearchResult<TopologySummary>), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> List([FromQuery]Search search)
        {
            var result = await _mgr.List(search);
            return Ok(result);
        }

        [HttpGet("api/topologies/all")]
        [ProducesResponseType(typeof(SearchResult<Topology>), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> ListAll([FromQuery]Search search)
        {
            var result = await _mgr.ListAll(search);
            return Ok(result);
        }

        [HttpPost("api/topology")]
        [ProducesResponseType(typeof(Topology), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Create([FromBody]NewTopology model)
        {
            Topology topo = await _mgr.Create(model);
            return Ok(topo);
        }

        [HttpPut("api/topology")]
        [ProducesResponseType(typeof(Topology), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Update([FromBody]ChangedTopology model)
        {
            Topology topo = await _mgr.Update(model);
            // Broadcast(topo.GlobalId, new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            _hub.Clients.Group(topo.GlobalId).TopoEvent(new BroadcastEvent<Topology>(User, "TOPO.UPDATED", topo));
            return Ok(topo);
        }

        [HttpGet("api/topology/{id}")]
        [ProducesResponseType(typeof(Topology), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Load([FromRoute]int id)
        {
            Topology topo = await _mgr.Load(id);
            return Ok(topo);
        }

        [HttpDelete("api/topology/{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            Topology topo = await _mgr.Delete(id);
            Log("deleted", topo);
            //Broadcast(topo.GlobalId, new BroadcastEvent<Topology>(User, "TOPO.DELETED", topo));
            return Ok(true);
        }

        [HttpGet("api/topology/{id}/publish")]
        [ProducesResponseType(typeof(TopologyState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Publish([FromRoute] int id)
        {
            TopologyState state = await _mgr.Publish(id, false);
            return Ok(state);
        }

        [HttpGet("api/topology/{id}/unpublish")]
        [ProducesResponseType(typeof(TopologyState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Unpublish([FromRoute] int id)
        {
            TopologyState state = await _mgr.Publish(id, true);
            return Ok(state);
        }

        [HttpGet("api/topology/{id}/lock")]
        [ProducesResponseType(typeof(TopologyState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Lock([FromRoute] int id)
        {
            TopologyState state = await _mgr.Lock(id, false);
            return Ok(state);
        }

        [HttpGet("api/topology/{id}/unlock")]
        [ProducesResponseType(typeof(TopologyState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Unlock([FromRoute] int id)
        {
            TopologyState state = await _mgr.Lock(id, true);
            return Ok(state);
        }

        [HttpGet("api/topology/{id}/share")]
        [ProducesResponseType(typeof(TopologyState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Share([FromRoute] int id)
        {
            TopologyState state = await _mgr.Share(id, false);
            return Ok(state);
        }

        [HttpGet("api/topology/{id}/unshare")]
        [ProducesResponseType(typeof(TopologyState), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Unshare([FromRoute] int id)
        {
            TopologyState state = await _mgr.Share(id, true);
            return Ok(state);
        }

        [HttpGet("api/worker/enlist/{code}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Enlist([FromRoute] string code)
        {
            return Ok(await _mgr.Enlist(code));
        }

        [HttpDelete("api/worker/delist/{workerId}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delist([FromRoute] int workerId)
        {
            return Ok(await _mgr.Delist(workerId));
        }

        [HttpGet("api/topology/{id}/isos")]
        [ProducesResponseType(typeof(VmOptions), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Isos([FromRoute] string id)
        {
            VmOptions result = await _pod.GetVmIsoOptions(id);
            return Ok(result);
        }

        [HttpGet("api/topology/{id}/nets")]
        [ProducesResponseType(typeof(VmOptions), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Nets([FromRoute] string id)
        {
            VmOptions result = await _pod.GetVmNetOptions(id);
            return Ok(result);
        }
    }
}