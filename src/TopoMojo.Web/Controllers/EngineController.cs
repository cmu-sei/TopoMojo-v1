// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize(Policy = "TrustedClients")]
    [ApiController]
    // [ApiExplorerSettings(IgnoreApi = true)]
    public class EngineController : _Controller
    {
        public EngineController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            EngineService engineService,
            IHypervisorService podService,
            IHubContext<AppHub, IHubEvent> hub,
            IDistributedCache cache
        ) : base(logger, identityResolver)
        {
            _engineService = engineService;
            _pod = podService;
            _hub = hub;
            _cache = cache;
        }

        private readonly IHypervisorService _pod;
        private readonly EngineService _engineService;
        private readonly IHubContext<AppHub, IHubEvent> _hub;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// List gamespaces published for a client.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("api/engine/workspaces")]
        public async Task<ActionResult<WorkspaceSummary>> List([FromQuery]Search search, CancellationToken ct)
        {
            var result = await _engineService.ListWorkspaces(search, ct);

            return Ok(result);
        }

        /// <summary>
        /// Create a new gamespace.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("api/engine/gamespace")]
        public async Task<ActionResult<GameState>> Launch([FromBody] GamespaceSpec model)
        {
            var result = await _engineService.Launch(model);

            result.WorkspaceDocument = ApplyPathBase(result.WorkspaceDocument);

            Log("launched", result);

            return Ok(result);
        }

        /// <summary>
        /// Delete a gamespace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("api/engine/gamespace/{id}")]
        public async Task<ActionResult> Destroy([FromRoute]string id)
        {
            await _engineService.Destroy(id);

            Log("destroyed", new GameState {GlobalId = id});

            return Ok();
        }

        /// <summary>
        /// Request vm console access ticket.
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <returns></returns>
        [HttpGet("api/engine/vm-console/{id}")]
        public async Task<ActionResult<ConsoleSummary>> Ticket([FromRoute]string id)
        {
            return Ok(await _engineService.Ticket(id));
        }

        /// <summary>
        /// Get a workspace's templates.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/engine/templates/{id}")]
        public async Task<ActionResult<string>> Templates([FromRoute]int id)
        {
            return Ok(await _engineService.GetTemplates(id));
        }

        /// <summary>
        /// Change vm state.
        /// </summary>
        /// <param name="vmAction"></param>
        /// <returns></returns>
        [HttpPut("api/engine/vm")]
        public async Task<IActionResult> ChangeVm([FromBody] VmAction vmAction)
        {
            return Ok(await _engineService.ChangeVm(vmAction));
        }

        // /// <summary>
        // /// Show usage information.
        // /// </summary>
        // /// <returns></returns>
        // [HttpGet("api/engine")]
        // public ActionResult<string> Usage()
        // {
        //     return Ok("See API documentation.");
        // }

        /// <summary>
        /// Register a gamespace on behalf of a user
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("api/engine/register")]
        public async Task<Registration> Register([FromBody]RegistrationRequest registration, CancellationToken ct)
        {
            var result = await _engineService.Register(registration);

            var opt = new DistributedCacheEntryOptions {
                SlidingExpiration = new TimeSpan(0, 0, 60)
            };

            await _cache.SetStringAsync(
                $"{TicketAuthentication.TicketCachePrefix}{result.Token}",
                $"{result.SubjectId}#{result.SubjectName}",
                //opt,
                ct
            );

            string payload = JsonSerializer.Serialize<Registration>(result);

            await _cache.SetStringAsync(
                $"{AppConstants.RegistrationCachePrefix}{result.Token}",
                payload,
                //opt,
                ct
            );

            // if url is just path, prepend host
            if (!result.RedirectUrl.Contains("://"))
            {
                result.RedirectUrl = string.Format("{0}://{1}{2}{3}",
                    Request.Scheme,
                    Request.Host,
                    Request.PathBase,
                    result.RedirectUrl
                );
            }

            return result;
        }

        /// <summary>
        /// Grade a challenge
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("api/engine/grade")]
        public async Task<Challenge> Grade([FromBody]Challenge challenge, CancellationToken ct)
        {
            return await _engineService.Grade(challenge);
        }

        /// <summary>
        /// Populate challenge hints
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("api/engine/hints")]
        public async Task<Challenge> Hints([FromBody]Challenge challenge, CancellationToken ct)
        {
            return await _engineService.Hints(challenge);
        }
    }
}
