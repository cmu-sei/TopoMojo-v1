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
    public class HomeController : Controller
    {
        public HomeController
        (
            ClientSettings settings
        )
        {
            _settings = settings;
        }

        private readonly ClientSettings _settings;

        public IActionResult Index()
        {
            ViewBag.Title = _settings.branding.applicationName;
            ViewBag.ClientSettings = JsonConvert.SerializeObject(_settings, Formatting.None);
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
