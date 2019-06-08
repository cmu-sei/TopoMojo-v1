// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TopoMojo.Core;
using TopoMojo.Services;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : _Controller
    {
        public AdminController(
            ChatService chatService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp,
            IHostingEnvironment env,
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
        private readonly IHostingEnvironment _env;
        private readonly TransferService _transferSvc;
        private readonly FileUploadOptions _fileUploadOptions;
        private readonly HubCache _hubCache;

        [AllowAnonymous]
        [HttpGet("api/version")]
        public string CommitVersion()
        {
            return Environment.GetEnvironmentVariable("COMMIT") ?? "no version info provided";
        }

        [HttpGet("/api/admin/getsettings")]
        [JsonExceptionFilter]
        public ActionResult<string> Settings()
        {
            string settings = "";
            string root = Path.Combine(_env.ContentRootPath, "appsettings.json");
            if (System.IO.File.Exists(root))
            {
                var appsettings = JObject.Parse(
                    System.IO.File.ReadAllText(root)
                );

                string target = Path.Combine(_env.ContentRootPath, $"appsettings.{_env.EnvironmentName}.json");
                if (System.IO.File.Exists(target))
                {
                    appsettings.Merge(
                        JObject.Parse(
                            System.IO.File.ReadAllText(target)
                        ),
                        new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Union
                        }
                    );
                }

                settings = appsettings.ToString(Formatting.Indented);
                return Json(appsettings);
            }
            return Json(settings);
        }

        [HttpPost("api/admin/savesettings")]
        [JsonExceptionFilter]
        public ActionResult<bool> Settings([FromBody]object settings)
        {
            try
            {
                var test = JObject.FromObject(settings);
                string target = Path.Combine(_env.ContentRootPath, $"appsettings.{_env.EnvironmentName}.json");
                System.IO.File.WriteAllText(target, test.ToString(Formatting.Indented));
            }
            catch //(Exception ex)
            {
                return Json(false);
            }
            return Json(true);
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
