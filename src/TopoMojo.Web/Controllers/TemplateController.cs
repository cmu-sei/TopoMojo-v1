using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp) : base(sp)
        {
            _mgr = templateManager;
            _pod = podManager;
            _hub = hub;
        }

        private readonly TemplateManager _mgr;
        private readonly IPodManager _pod;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [HttpGet("api/templates")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<TemplateSummary>>> List(Search search)
        {
            var result = await _mgr.List(search);
            return Ok(result);
        }

        [HttpGet("api/template/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Template>> Load(int id)
        {
            var result = await _mgr.Load(id);
            return Ok(result);
        }

        [HttpPut("api/template")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Template>> Update([FromBody]ChangedTemplate template)
        {
            var result = await _mgr.Update(template);
            SendBroadcast(result, "updated");
            return Ok(result);
        }

        [HttpDelete("api/template/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _mgr.Delete(id);
            SendBroadcast(result, "removed");
            return Ok(true);
        }

        [HttpPost("api/template/link")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Template>> Link([FromBody]TemplateLink link)
        {
            var result = await _mgr.Link(link);
            SendBroadcast(result, "added");
            return Ok(result);
        }

        [HttpPost("api/template/unlink")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Template>> UnLink([FromBody]TemplateLink link)
        {
            var result = await _mgr.Unlink(link);
            SendBroadcast(result, "updated");
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("api/template/{id}/detailed")]
        [JsonExceptionFilter]
        public async Task<ActionResult<TemplateDetail>> LoadDetail(int id)
        {
            var result = await _mgr.LoadDetail(id);
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("api/template/detailed")]
        [JsonExceptionFilter]
        public async Task<ActionResult<TemplateDetail>> Create([FromBody]TemplateDetail model)
        {
            var result = await _mgr.Create(model);
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("api/template/detail")]
        [JsonExceptionFilter]
        public async Task<ActionResult<TemplateDetail>> Configure([FromBody]TemplateDetail template)
        {
            var result = await _mgr.Configure(template);
            return Ok(result);
        }

        private void SendBroadcast(Template template, string action)
        {
            _hub.Clients
                .Group(template.TopologyGlobalId)
                .TemplateEvent(
                    new BroadcastEvent<Core.Models.Template>(
                        User,
                        "TEMPLATE." + action.ToUpper(),
                        template
                    )
                );
        }
    }
}
