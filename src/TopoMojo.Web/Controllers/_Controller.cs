using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models;

namespace TopoMojo.Controllers
{
    public class _Controller : Controller
    {
        public _Controller(IServiceProvider sp)
        {
            _logger = sp.GetService<ILoggerFactory>().CreateLogger(this.GetType());
            _options = sp.GetService<IOptions<ControlOptions>>().Value;
            _profileResolver = sp.GetRequiredService<IProfileResolver>();
        }

        protected Profile _profile;
        protected readonly IProfileResolver _profileResolver;
        protected readonly ILogger _logger;
        protected readonly ControlOptions _options;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _profile = _profileResolver.Profile;
            ViewBag.AppName = _options.ApplicationName;
        }

        internal void Log(string action, dynamic item, string msg = "")
        {
            string itemType = (item != null) ? item.GetType().Name : "null";
            string itemName = (item != null) ? item.Name : "null";
            string itemId = (item != null) ? item.Id.ToString() : "";
            string entry = String.Format("{0} [{1}] {2} {3} {4} [{5}] {6}",
                _profile.Name, _profile.GlobalId, action, itemType, itemName, itemId, msg);
            _logger.LogInformation(entry);
        }

    }
}