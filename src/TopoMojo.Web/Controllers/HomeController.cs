using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        public HomeController
        (
            ClientAuthenticationSettings settings
        )
        {
            _settings = settings;
        }

        private readonly ClientAuthenticationSettings _settings;

        public IActionResult Index()
        {
            ViewBag.UserManagerSettings = JsonConvert.SerializeObject(_settings, Formatting.None);
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
