// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class TemplateController : _Controller
    {
        public TemplateController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            TemplateService templateService,
            IHypervisorService podService,
            IHubContext<TopologyHub, ITopoEvent> hub
        ) : base(logger, identityResolver)
        {
            _templateService = templateService;
            _pod = podService;
            _hub = hub;
        }

        private readonly TemplateService _templateService;
        private readonly IHypervisorService _pod;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        /// <summary>
        /// List templates.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("api/templates")]
        public async Task<ActionResult<TemplateSummary[]>> List([FromQuery]Search search, CancellationToken ct)
        {
            var result = await _templateService.List(search, ct);

            return Ok(result);
        }

        /// <summary>
        /// Load a template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/template/{id}")]
        public async Task<ActionResult<Template>> Load(int id)
        {
            var result = await _templateService.Load(id);

            return Ok(result);
        }

        /// <summary>
        /// Update a template.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPut("api/template")]
        public async Task<ActionResult> Update([FromBody]ChangedTemplate template)
        {
            var result = await _templateService.Update(template);
            SendBroadcast(result, "updated");
            return Ok();
        }

        /// <summary>
        /// Delete a template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("api/template/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _templateService.Delete(id);

            SendBroadcast(result, "removed");

            return Ok();
        }

        /// <summary>
        /// Create a new template linked to a parent template.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        [HttpPost("api/template")]
        public async Task<ActionResult<Template>> Link([FromBody]TemplateLink link)
        {
            var result = await _templateService.Link(link);

            SendBroadcast(result, "added");

            return Ok(result);
        }

        /// <summary>
        /// Detach a template from its parent.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        [HttpPost("api/template/unlink")]
        public async Task<ActionResult<Template>> UnLink([FromBody]TemplateLink link)
        {
            var result = await _templateService.Unlink(link);

            SendBroadcast(result, "updated");

            return Ok(result);
        }

        /// <summary>
        /// Load template detail.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("api/template-detail/{id}")]
        public async Task<ActionResult<TemplateDetail>> LoadDetail(int id)
        {
            var result = await _templateService.LoadDetail(id);

            return Ok(result);
        }

        /// <summary>
        /// Create new template with detail.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("api/template-detail")]
        public async Task<ActionResult<TemplateDetail>> Create([FromBody]TemplateDetail model)
        {
            var result = await _templateService.Create(model);

            return Ok(result);
        }

        /// <summary>
        /// Update template detail.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("api/template-detail")]
        public async Task<ActionResult> Configure([FromBody]TemplateDetail template)
        {
            var result = await _templateService.Configure(template);

            return Ok();
        }

        private void SendBroadcast(Template template, string action)
        {
            _hub.Clients
                .Group(template.WorkspaceGlobalId ?? Guid.Empty.ToString())
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
