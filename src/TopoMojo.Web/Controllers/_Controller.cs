using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Core;
using TopoMojo.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TopoMojo.Data;
using TopoMojo.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace TopoMojo.Controllers
{
    public class _Controller : Controller
    {
        // public _Controller(
        //     IOptions<ApplicationOptions> optionsAccessor,
        //     ILoggerFactory mill,
        //     IUserManager userManager
        // )
        // {
        //     _options = optionsAccessor.Value;
        //     _logger = mill.CreateLogger(this.GetType());
        //     _userManager = userManager;
        // }
        public _Controller(IServiceProvider sp)
        {
            _logger = sp.GetService<ILoggerFactory>().CreateLogger(this.GetType());
            _userManager = sp.GetService<UserManager<ApplicationUser>>();
            _options = sp.GetService<IOptions<ApplicationOptions>>().Value;
        }

        protected readonly ILogger _logger;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected ApplicationUser _user = null;

        protected readonly ApplicationOptions _options;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.AppName = _options.Site.Name;
            _user = GetCurrentUserAsync().Result;
        }

        internal void Log(string action, dynamic item, string msg = "")
        {
            ApplicationUser user = GetCurrentUserAsync().Result;
            string itemType = (item != null) ? item.GetType().Name : "null";
            string itemName = (item != null) ? item.Name : "null";
            string itemId = (item != null) ? item.Id : "";
            string entry = String.Format("{0} [{1}] {2} {3} {4} [{5}] {6}",
                user.UserName, user.Id, action, itemType, itemName, itemId, msg);
            _logger.LogInformation(entry);
        }

        internal async Task<ApplicationUser> GetCurrentUserAsync()
        {
            if (_user == null)
            {
                _user = await _userManager.GetUserAsync(HttpContext.User);
                if (_user == null)
                {
                    _user = new ApplicationUser{ Id = "", UserName = "anonymous user"};
                }
            }
            return _user;
        }

        internal bool AuthorizedForRoom(string id)
        {
            //TODO: lookup TopoId from IsolationTag (for now they are the same)
            ApplicationUser user = GetCurrentUserAsync().Result;
            return user.IsAdmin; //|| user.TopoId == id;
        }
    }
}