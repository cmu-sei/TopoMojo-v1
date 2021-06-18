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
using System.Threading.Tasks;

namespace TopoMojo.Web.Controllers
{
    public class _Controller : Controller
    {
        public _Controller(
            ILogger logger,
            IIdentityResolver identityResolver,
            params IModelValidator[] validators
        )
        {
            _logger = logger;
            _identityResolver = identityResolver;
            _validators = validators;
        }

        protected User Actor;
        protected User _user;
        protected Client _client;
        protected readonly IIdentityResolver _identityResolver;
        private readonly IModelValidator[] _validators;
        protected readonly ILogger _logger;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Actor = User.ToModel();
            // _user = _identityResolver.User;
            // _client = _identityResolver.Client;
        }

        protected async Task Validate(object model)
        {
            foreach (var v in _validators)
                await v.Validate(model);

        }

        protected void AuthorizeAll(params Func<Boolean>[] requirements)
        {
            bool valid = true;

            foreach(var requirement in requirements)
                valid &= requirement.Invoke();

            if (valid.Equals(false))
                throw new ActionForbidden();
        }

        protected void AuthorizeAny(params Func<Boolean>[] requirements)
        {
            bool valid = false;

            foreach(var requirement in requirements)
            {
                valid |= requirement.Invoke();
                if (valid) break;
            }

            if (valid.Equals(false))
                throw new ActionForbidden();
        }

        internal void Log(string action, dynamic item, string msg = "")
        {
            string entry = String.Format("{0} [{1}] {2} {3} {4} [{5}] {6}",
                _user?.Name, _user?.GlobalId, action, item?.GetType().Name, item?.Name, item?.Id, msg);

            _logger.LogInformation(entry);
        }

        /// <summary>
        /// Apply PathBase to urls generated in service
        /// </summary>
        /// <param name="target"></param>
        internal string ApplyPathBase(string target)
        {
            if (
                !string.IsNullOrEmpty(Request.PathBase)
                && !string.IsNullOrEmpty(target)
                && !target.Contains("://")
            )
            {
                return Request.PathBase + target;
            }

            return target;
        }

    }

    public enum AuthRequirement
    {
        None,
        Self
    }
}
