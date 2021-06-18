// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Hubs;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    public class AdminController : _Controller
    {
        public AdminController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            IHubContext<AppHub, IHubEvent> hub,
            TransferService transferSvc,
            FileUploadOptions fileUploadOptions,
            JanitorService janitor,
            HubCache hubCache
        ) : base(logger, identityResolver)
        {
            _transferSvc = transferSvc;
            _uploadOptions = fileUploadOptions;
            _hub = hub;
            _hubCache = hubCache;
            _janitor = janitor;
        }

        private readonly IHubContext<AppHub, IHubEvent> _hub;
        private readonly TransferService _transferSvc;
        private readonly FileUploadOptions _uploadOptions;
        private readonly HubCache _hubCache;
        private readonly JanitorService _janitor;

        /// <summary>
        /// Show application version info.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("api/version")]
        public IActionResult CommitVersion()
        {
            return Ok(new { Version = Environment.GetEnvironmentVariable("COMMIT") ?? "no version info provided"});
        }

        /// <summary>
        /// Post an announcement to users.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost("api/admin/announce")]
        public async Task<ActionResult<bool>> Announce([FromBody]string text)
        {
            await Task.Run(() => SendBroadcast(text));
            return Ok(true);
        }

        /// <summary>
        /// Generate an export package.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost("api/admin/export")]
        [ProducesResponseType(typeof(string[]), 200)]
        public async Task<ActionResult> Export([FromBody] int[] ids)
        {
            string srcPath = _uploadOptions.TopoRoot;
            string destPath = Path.Combine(
                _uploadOptions.TopoRoot,
                "_export"
            );
            await _transferSvc.Export(ids, srcPath, destPath);

            return Ok();
        }

        /// <summary>
        /// Initiate import process.
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/admin/import")]
        public async Task<ActionResult<string[]>> Import()
        {
            return Ok(await _transferSvc.Import(
                _uploadOptions.TopoRoot,
                _uploadOptions.DocRoot
            ));
        }

        /// <summary>
        /// Show online users.
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/admin/live")]
        public ActionResult<CachedConnection[]> LiveUsers()
        {
            return Ok(_hubCache.Connections.Values);
        }

        /// <summary>
        /// Run clean up tasks
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPost("api/admin/janitor")]
        public async Task<ActionResult<JanitorReport[]>> Cleanup([FromBody]JanitorOptions options = null)
        {
            return Ok(await _janitor.Cleanup(options));
        }

        private void SendBroadcast(string text = "")
        {
            _hub.Clients.All.GlobalEvent(
                    new BroadcastEvent<string>(
                        User,
                        "GLOBAL.ANNOUNCE",
                        text
                    )
                );
        }
    }
}
