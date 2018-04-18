using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    public class ConsoleController : Controller
    {
        public ConsoleController (
            ClientSettings settings
        ) {
            _settings = settings;
        }

        private readonly ClientSettings _settings;

        [HttpGet("console/{id}/{name?}")]
        public IActionResult Index([FromRoute] string id, [FromRoute] string name)
        {
            ViewBag.Title = "console: " + name.Untagged();

            var model = new ConsoleDetail {
                Id = id,
                Key = $"{_settings.Oidc.storageKeyPrefix}:{_settings.Oidc.authority}:{_settings.Oidc.client_id}"
            };

            return View("Index", model);
        }
    }
}
