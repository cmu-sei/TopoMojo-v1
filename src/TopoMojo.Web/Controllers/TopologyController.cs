using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class TopologyController : _Controller
    {
        public TopologyController(
            TopologyManager topologyManager,
            IServiceProvider sp) : base(sp)
        {
            _mgr = topologyManager;
        }

        private readonly TopologyManager _mgr;

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Create([FromBody]Topology topo)
        {
            return Json(await _mgr.Create(topo));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Update([FromBody]Topology topo)
        {
            return Json(await _mgr.Update(topo));
        }

        [HttpGet("{id}")]
        [JsonExceptionFilterAttribute]
        public async Task<IActionResult> Load([FromRoute]int id)
        {
            return Json(await _mgr.LoadAsync(id));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> List([FromBody]Search<TopoSummary> search)
        {
            return Json(await _mgr.ListAsync(search));
        }

        [HttpGet("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Templates([FromRoute]int id)
        {
            return Json(await _mgr.ListTemplates(id));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> AddTemplate([FromBody]TemplateReference tref)
        {
            return Json(await _mgr.AddTemplate(tref));
        }
        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> UpdateTemplate([FromBody]TemplateReference tref)
        {
            return Json(await _mgr.UpdateTemplate(tref));
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> RemoveTemplate([FromRoute]int id)
        {
            return Json(await _mgr.RemoveTemplate(id));
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> CloneTemplate([FromRoute]int id)
        {
            return Json(await _mgr.CloneTemplate(id));
        }

        [HttpGetAttribute("{id}")]
        [JsonExceptionFilterAttribute]
        public async Task<IActionResult> Members([FromRoute] int id)
        {
            return Json(await _mgr.Members(id));
        }


        // [HttpPost]
        // [JsonExceptionFilter]
        // public async Task<IActionResult> Create([FromBody]TopologyModel model)
        // {
        //     return Json(await _mgr.Create(model));
        // }

        // [HttpPost]
        // [JsonExceptionFilter]
        // public async Task<IActionResult> Save([FromBody]Topology Topology)
        // {
        //     return Json(await _mgr.Save(Topology));
        // }


    }

}