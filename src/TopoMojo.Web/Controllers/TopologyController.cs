using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class TopologyController : _Controller
    {
        public TopologyController(
            TopologyManager topologyManager,
            IPodManager podManager,
            IHostingEnvironment env,
            IServiceProvider sp) : base(sp)
        {
            _pod = podManager;
            _mgr = topologyManager;
            _env = env;
        }

        private readonly IPodManager _pod;
        private readonly TopologyManager _mgr;
        private readonly IHostingEnvironment _env;

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<Topology> Create([FromBody]Topology topo)
        {
            return await _mgr.Create(topo);
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<Topology> Update([FromBody]Topology topo)
        {
            return await _mgr.Update(topo);
        }

        [HttpGet("{id}")]
        [JsonExceptionFilterAttribute]
        public async Task<Topology> Load([FromRoute]int id)
        {
            return await _mgr.LoadAsync(id);
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilterAttribute]
        public async Task<bool> Delete([FromRoute]int id)
        {
            Topology topo = await _mgr.LoadAsync(id);
            foreach (Vm vm in await _pod.Find(topo.GlobalId))
                await _pod.Delete(vm.Id);
            return await _mgr.DeleteAsync(topo);
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<SearchResult<TopoSummary>> List([FromBody]Search search)
        {
            return await _mgr.ListAsync(search);
        }

        [HttpGet("{id}")]
        [JsonExceptionFilter]
        public async Task<TemplateReference[]> Templates([FromRoute]int id)
        {
            return await _mgr.ListTemplates(id);
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<TemplateReference> AddTemplate([FromBody]TemplateReference tref)
        {
            return await _mgr.AddTemplate(tref);
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<TemplateReference> UpdateTemplate([FromBody]TemplateReference tref)
        {
            return await _mgr.UpdateTemplate(tref);
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilter]
        public async Task<bool> RemoveTemplate([FromRoute]int id)
        {
            return await _mgr.RemoveTemplate(id);
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<TemplateReference> CloneTemplate([FromRoute]int id)
        {
            return await _mgr.CloneTemplate(id);
        }

        [HttpGetAttribute("{id}")]
        [JsonExceptionFilterAttribute]
        public async Task<Core.Permission[]> Members([FromRoute] int id)
        {
            return await _mgr.Members(id);
        }

    }

}