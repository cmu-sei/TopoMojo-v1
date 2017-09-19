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
using TopoMojo.Core.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class TemplateController : _Controller
    {
        public TemplateController(
            TemplateManager templateManager,
            IPodManager podManager,
            IServiceProvider sp) : base(sp)
        {
            _mgr = templateManager;
            _pod = podManager;
        }

        private readonly TemplateManager _mgr;
        private readonly IPodManager _pod;

        [HttpGet("api/templates")]
        [ProducesResponseType(typeof(SearchResult<TemplateSummary>), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> List([FromQuery]Search search)
        {
            var result = await _mgr.List(search);
            return Ok(result);
        }

        // [HttpGet("api/templates/detail")]
        // [ProducesResponseType(typeof(SearchResult<TemplateDetail>), 200)]
        // [JsonExceptionFilter]
        // public async Task<IActionResult> ListDetail([FromQuery]Search search)
        // {
        //     var result = await _mgr.ListDetail(search);
        //     return Ok(result);
        // }

        [HttpGet("api/template/{id}")]
        [ProducesResponseType(typeof(Template), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Load([FromRoute]int id)
        {
            var result = await _mgr.Load(id);
            return Ok(result);
        }

        [HttpGet("api/template/{id}/detailed")]
        [ProducesResponseType(typeof(TemplateDetail), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> LoadDetail([FromRoute]int id)
        {
            var result = await _mgr.LoadDetail(id);
            return Ok(result);
        }

        [HttpPost("api/template/create")]
        [ProducesResponseType(typeof(TemplateDetail), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Create([FromBody]TemplateDetail model)
        {
            var result = await _mgr.Create(model);
            return Ok(result);
        }

        [HttpPut("api/template/configure")]
        [ProducesResponseType(typeof(TemplateDetail), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Configure([FromBody]TemplateDetail template)
        {
            var result = await _mgr.Configure(template);
            return Ok(result);
        }

        [HttpGet("api/template/{id}/link/{topoId}")]
        [ProducesResponseType(typeof(Template), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Link([FromRoute]int id, [FromRoute]int topoId)
        {
            var result = await _mgr.Link(id, topoId);
            //TODO: Broadcast
            return Ok(result);
        }

        [HttpGet("api/template/{id}/unlink")]
        [ProducesResponseType(typeof(Template), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> UnLink([FromRoute]int id)
        {
            var result = await _mgr.Unlink(id);
            //TODO: Broadcast
            return Ok(result);
        }

        [HttpPut("api/template")]
        [ProducesResponseType(typeof(Template), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Update([FromBody]ChangedTemplate template)
        {
            var result = await _mgr.Update(template);
            //TODO: Broadcast
            return Ok(result);
        }

        [HttpDelete("api/template/{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            await _mgr.Delete(id);
            //TODO: Broadcast
            return Ok(true);
        }

    }
}