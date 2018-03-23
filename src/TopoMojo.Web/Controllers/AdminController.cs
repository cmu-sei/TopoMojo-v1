using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
    public class AdminController : _Controller
    {
        public AdminController(
            ChatService chatService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp,
            IHostingEnvironment env,
            TransferService transferSvc,
            FileUploadOptions fileUploadOptions
        ) : base(sp)
        {
            _chatService = chatService;
            _transferSvc = transferSvc;
            _fileUploadOptions = fileUploadOptions;
            _hub = hub;
            _env = env;
        }

        private readonly ChatService _chatService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;
        private readonly IHostingEnvironment _env;
        private readonly TransferService _transferSvc;
        private readonly FileUploadOptions _fileUploadOptions;

        [HttpGet("/api/admin/getsettings")]
        [JsonExceptionFilter]
        public IActionResult Settings()
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
        public IActionResult Settings([FromBody]object settings)
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
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Announce([FromBody]string text)
        {
            await Task.Run(() => SendBroadcast(text));
            return Ok(true);
        }

        [HttpPost("api/admin/export")]
        [ProducesResponseType(typeof(string[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Export([FromBody]int[] ids)
        {
            string destPath = Path.Combine(
                _fileUploadOptions.TopoRoot,
                "exports",
                DateTime.Now.ToString("s").Replace(":", "")
            );
            string docPath = Path.Combine(_env.WebRootPath, "docs");
            await _transferSvc.Export(ids, destPath, docPath);
            string[] results = new string[] {
                "Make note of your backend export folder:",
                destPath
            };
            return Ok(results);
        }

        [HttpGet("api/admin/import")]
        [ProducesResponseType(typeof(string[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Import()
        {
            string destPath = _fileUploadOptions.TopoRoot;
            string docPath = Path.Combine(_env.WebRootPath, "docs");
            return Ok(await _transferSvc.Import(destPath, docPath));
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