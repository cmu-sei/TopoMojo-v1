// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TopoMojo.Middleware
{
    public class QuerystringBearerTokenMiddleware
    {
        public QuerystringBearerTokenMiddleware(
            RequestDelegate next,
            ILogger<HeaderInspectionMiddleware> logger,
            string tokenName = "access_token"
        ){
            _next = next;
            _logger = logger;
            _tokenName = tokenName;
        }

        private readonly string _tokenName;
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"])
                && context.Request.Query[_tokenName].Any())
            {
                string token = context.Request.Query[_tokenName].FirstOrDefault();

                if (!String.IsNullOrWhiteSpace(token))
                    context.Request.Headers.Add("Authorization", new[] { $"Bearer {token}" });
            }
            await _next(context);
        }
    }
}
