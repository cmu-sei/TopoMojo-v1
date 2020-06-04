// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
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
        public _Controller(
            ILogger logger,
            IIdentityResolver identityResolver
        )
        {
            _logger = logger;
            _identityResolver = identityResolver;
        }

        protected User _user;
        protected Client _client;
        protected readonly IIdentityResolver _identityResolver;
        protected readonly ILogger _logger;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _user = _identityResolver.User;
            _client = _identityResolver.Client;
        }

        internal void Log(string action, dynamic item, string msg = "")
        {
            string entry = String.Format("{0} [{1}] {2} {3} {4} [{5}] {6}",
                _user?.Name, _user?.GlobalId, action, item?.GetType().Name, item?.Name, item?.Id, msg);

            _logger.LogInformation(entry);
        }

    }
}
