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
using TopoMojo.Hubs;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [ApiController]
    [Authorize]
    public class TemplateController : _Controller
    {
        public TemplateController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            TemplateService templateService,
            IHypervisorService podService,
            IHubContext<AppHub, IHubEvent> hub
        ) : base(logger, identityResolver)
        {
            _svc = templateService;
            _pod = podService;
            _hub = hub;
        }

        private readonly TemplateService _svc;
        private readonly IHypervisorService _pod;
        private readonly IHubContext<AppHub, IHubEvent> _hub;

        /// <summary>
        /// List templates.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <remarks>
        /// Filter for `published`.
        /// Non-Admins always get `published`.
        /// Admins can use `pid=id` for parents (id = 0) or children (id > 0)
        /// </remarks>
        /// <returns>TemplateSummary[]</returns>
        [HttpGet("api/templates")]
        [Authorize]
        public async Task<ActionResult<TemplateSummary[]>> List([FromQuery]TemplateSearch search, CancellationToken ct)
        {
            await Validate(search);

            AuthorizeAll();

            var result = await _svc.List(search, Actor.IsAdmin, ct);

            return Ok(result);
        }

        /// <summary>
        /// Load a template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/template/{id}")]
        [Authorize]
        public async Task<ActionResult<Template>> Load(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            return Ok(
                await _svc.Load(id)
            );
        }

        /// <summary>
        /// Update a template.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("api/template")]
        public async Task<ActionResult> Update([FromBody]ChangedTemplate model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(model.GlobalId, Actor.Id).Result
            );

            var result = await _svc.Update(model);

            SendBroadcast(result, "updated");

            return Ok();
        }

        /// <summary>
        /// Delete a template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("api/template/{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            await Validate(new Entity { Id = id });

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(id, Actor.Id).Result
            );

            var result = await _svc.Delete(id);

            SendBroadcast(result, "removed");

            return Ok();
        }

        /// <summary>
        /// Create a new template linked to a parent template.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("api/template")]
        public async Task<ActionResult<Template>> Link([FromBody]TemplateLink model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEditWorkspace(model.WorkspaceId, Actor.Id).Result
            );

            var result = await _svc.Link(model, Actor.IsCreator);

            SendBroadcast(result, "added");

            return Ok(result);
        }

        /// <summary>
        /// Detach a template from its parent.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("api/template/unlink")]
        public async Task<ActionResult<Template>> UnLink([FromBody]TemplateLink model)
        {
            await Validate(model);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => _svc.CanEdit(model.TemplateId, Actor.Id).Result
            );

            var result = await _svc.Unlink(model);

            SendBroadcast(result, "updated");

            return Ok(result);
        }

        /// <summary>
        /// Load template detail.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/template-detail/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<TemplateDetail>> LoadDetail(string id)
        {
            await Validate(new Entity{ Id = id });

            AuthorizeAll();

            return Ok(
                await _svc.LoadDetail(id)
            );
        }

        /// <summary>
        /// Create new template with detail.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("api/template-detail")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<TemplateDetail>> Create([FromBody]TemplateDetail model)
        {
            await Validate(model);

            AuthorizeAll();

            return Ok(
                await _svc.Create(model)
            );
        }

        /// <summary>
        /// Update template detail.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("api/template-detail")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Configure([FromBody]TemplateDetail model)
        {
            await Validate(model);

            AuthorizeAll();

            var result = await _svc.Configure(model);

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