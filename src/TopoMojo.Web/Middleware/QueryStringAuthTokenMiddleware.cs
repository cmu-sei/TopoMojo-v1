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
            ILogger<HeaderInspectionMiddleware> logger
        ){
            _next = next;
            _logger = logger;
        }

        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"])
                && context.Request.Query["bearer"].Any())
            {
                string token = context.Request.Query["bearer"].FirstOrDefault();

                if (!String.IsNullOrWhiteSpace(token))
                    context.Request.Headers.Add("Authorization", new[] { $"Bearer {token}" });
            }
            await _next(context);
        }
    }
}
