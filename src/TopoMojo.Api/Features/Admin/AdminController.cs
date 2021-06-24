// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using TopoMojo.Api.Hubs;
using TopoMojo.Api.Models;
using TopoMojo.Api.Services;

namespace TopoMojo.Api.Controllers
{
    [Authorize(AppConstants.AdminOnlyPolicy)]
    [ApiController]
    public class AdminController : _Controller
    {
        public AdminController(
            ILogger<AdminController> logger,
            IHubContext<AppHub, IHubEvent> hub,
            TransferService transferSvc,
            FileUploadOptions fileUploadOptions,
            JanitorService janitor,
            HubCache hubCache
        ) : base(logger, hub)
        {
            _transferSvc = transferSvc;
            _uploadOptions = fileUploadOptions;
            _hubCache = hubCache;
            _janitor = janitor;
        }

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
        [SwaggerOperation(OperationId = "LoadDocument")]
        public ActionResult<AppVersionInfo> GetAppVersionInfo()
        {
            return Ok(new AppVersionInfo
            {
                Commit = Environment.GetEnvironmentVariable("COMMIT")
                    ?? "no version info provided"
            });
        }

        /// <summary>
        /// Post an announcement to users.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost("api/admin/announce")]
        [SwaggerOperation(OperationId = "PostAnnouncement")]
        public async Task<ActionResult<bool>> PostAnnouncement([FromBody]string text)
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
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> ExportWorkspaces([FromBody] string[] ids)
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
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<string[]>> ImportWorkspaces()
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
        [SwaggerOperation(OperationId = "ListActiveUsers")]
        public ActionResult<CachedConnection[]> ListActiveUsers()
        {
            return Ok(_hubCache.Connections.Values);
        }

        /// <summary>
        /// Run clean up tasks
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPost("api/admin/janitor")]
        [SwaggerOperation(OperationId = "RunJanitorCleanup")]
        public async Task<ActionResult<JanitorReport[]>> RunJanitorCleanup([FromBody]JanitorOptions options = null)
        {
            return Ok(await _janitor.Cleanup(options));
        }

        private void SendBroadcast(string text = "")
        {
            Hub.Clients.All.GlobalEvent(
                    new BroadcastEvent<string>(
                        User,
                        "GLOBAL.ANNOUNCE",
                        text
                    )
                );
        }
    }
}
