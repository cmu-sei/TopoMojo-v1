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
        //[HttpGet("api/console/{id}/{name?}")]
        public IActionResult Index([FromRoute] string id, [FromRoute] string name)
        {
            ViewBag.Title = "console: " + name.Untagged();
            return View("Index", id);
        }
    }
}
