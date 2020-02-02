// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class TemplateController : _Controller
    {
        public TemplateController(
            TemplateService templateService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp) : base(sp)
        {
            _templateService = templateService;
            _pod = podService;
            _hub = hub;
        }

        private readonly TemplateService _templateService;
        private readonly IHypervisorService _pod;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [HttpGet("api/templates")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<TemplateSummary>>> List(Search search)
        {
            var result = await _templateService.List(search);
            return Ok(result);
        }

        [HttpGet("api/template/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Template>> Load(int id)
        {
            var result = await _templateService.Load(id);
            return Ok(result);
        }

        [HttpPut("api/template")]
        [JsonExceptionFilter]
        public async Task<ActionResult> Update([FromBody]ChangedTemplate template)
        {
            var result = await _templateService.Update(template);
            SendBroadcast(result, "updated");
            return Ok();
        }

        [HttpDelete("api/template/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _templateService.Delete(id);
            SendBroadcast(result, "removed");
            return Ok(true);
        }

        [HttpPost("api/template/link")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Template>> Link([FromBody]TemplateLink link)
        {
            var result = await _templateService.Link(link);
            SendBroadcast(result, "added");
            return Ok(result);
        }

        [HttpPost("api/template/unlink")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Template>> UnLink([FromBody]TemplateLink link)
        {
            var result = await _templateService.Unlink(link);
            SendBroadcast(result, "updated");
            return Ok(result);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("api/template/{id}/detailed")]
        [JsonExceptionFilter]
        public async Task<ActionResult<TemplateDetail>> LoadDetail(int id)
        {
            var result = await _templateService.LoadDetail(id);
            return Ok(result);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("api/template/detailed")]
        [JsonExceptionFilter]
        public async Task<ActionResult<TemplateDetail>> Create([FromBody]TemplateDetail model)
        {
            var result = await _templateService.Create(model);
            return Ok(result);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("api/template/detail")]
        [JsonExceptionFilter]
        public async Task<ActionResult> Configure([FromBody]TemplateDetail template)
        {
            var result = await _templateService.Configure(template);
            return Ok();
        }

        private void SendBroadcast(Template template, string action)
        {
            _hub.Clients
                .Group(template.TopologyGlobalId ?? Guid.Empty.ToString())
                .TemplateEvent(
                    new BroadcastEvent<Template>(
                        User,
                        "TEMPLATE." + action.ToUpper(),
                        template
                    )
                );
        }
    }
}
