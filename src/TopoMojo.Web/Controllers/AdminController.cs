// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Core;
using TopoMojo.Services;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : _Controller
    {
        public AdminController(
            ChatService chatService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp,
            IWebHostEnvironment env,
            TransferService transferSvc,
            FileUploadOptions fileUploadOptions,
            HubCache hubCache
        ) : base(sp)
        {
            _chatService = chatService;
            _transferSvc = transferSvc;
            _fileUploadOptions = fileUploadOptions;
            _hub = hub;
            _env = env;
            _hubCache = hubCache;
        }

        private readonly ChatService _chatService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;
        private readonly IWebHostEnvironment _env;
        private readonly TransferService _transferSvc;
        private readonly FileUploadOptions _fileUploadOptions;
        private readonly HubCache _hubCache;

        [AllowAnonymous]
        [HttpGet("api/version")]
        public IActionResult CommitVersion()
        {
            return Ok(new { Version = Environment.GetEnvironmentVariable("COMMIT") ?? "no version info provided"});
        }


        [HttpPost("api/admin/announce")]
        [JsonExceptionFilter]
        public async Task<ActionResult<bool>> Announce([FromBody]string text)
        {
            await Task.Run(() => SendBroadcast(text));
            return Ok(true);
        }

        [HttpPost("api/admin/export")]
        [ProducesResponseType(typeof(string[]), 200)]
        [JsonExceptionFilter]
        public async Task<ActionResult> Export([FromBody] int[] ids)
        {
            string srcPath = _fileUploadOptions.TopoRoot;
            string destPath = Path.Combine(
                _fileUploadOptions.TopoRoot,
                "_export"
            );
            await _transferSvc.Export(ids, srcPath, destPath);

            return Ok();
        }

        [HttpGet("api/admin/import")]
        [JsonExceptionFilter]
        public async Task<ActionResult<string[]>> Import()
        {
            string destPath = _fileUploadOptions.TopoRoot;
            string docPath = Path.Combine(_env.WebRootPath, "_docs");
            return Ok(await _transferSvc.Import(destPath, docPath));
        }

        [HttpGet("api/admin/live")]
        [JsonExceptionFilter]
        public ActionResult<CachedConnection[]> LiveUsers()
        {
            return Ok(_hubCache.Connections.Values);
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
