using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            // Jam.Accounts.AccountOptions accountOptions
        )
        {
            _settings = settings;
            // _accountOptions = accountOptions;
        }

        private readonly ClientSettings _settings;
        // private readonly Jam.Accounts.AccountOptions _accountOptions;

        public IActionResult Index()
        {
            ViewBag.Title = _settings.Branding.ApplicationName;
            // _settings.Login.AllowedDomains = _accountOptions.Registration.AllowedDomains;
            // _settings.Login.PasswordComplexity = _accountOptions.Password.ComplexityText;
            ViewBag.ClientSettings = _settings.ToUglyJson();
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
