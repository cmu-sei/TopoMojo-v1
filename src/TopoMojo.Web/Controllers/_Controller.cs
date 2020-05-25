// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Web.Controllers
{
    public class _Controller : Controller
    {
        public _Controller(IServiceProvider sp)
        {
            _logger = sp.GetService<ILoggerFactory>().CreateLogger(this.GetType());
            _identityResolver = sp.GetRequiredService<IIdentityResolver>();
        }

        protected User _user;
        protected readonly IIdentityResolver _identityResolver;
        protected readonly ILogger _logger;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _user = _identityResolver.User;
        }

        internal void Log(string action, dynamic item, string msg = "")
        {
            string entry = String.Format("{0} [{1}] {2} {3} {4} [{5}] {6}",
                _user?.Name, _user?.GlobalId, action, item?.GetType().Name, item?.Name, item?.Id, msg);

            _logger.LogInformation(entry);
        }

    }
}
