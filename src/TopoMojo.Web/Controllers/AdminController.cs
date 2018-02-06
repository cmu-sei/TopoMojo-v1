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
            IHostingEnvironment env
        ) : base(sp)
        {
            _chatService = chatService;
            _hub = hub;
            _env = env;
        }

        private readonly ChatService _chatService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;
        private readonly IHostingEnvironment _env;


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