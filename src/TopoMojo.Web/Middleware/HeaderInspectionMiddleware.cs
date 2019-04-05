using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TopoMojo.Middleware
{
    public class HeaderInspectionMiddleware
    {
        public HeaderInspectionMiddleware(
            RequestDelegate next,
            ILogger<HeaderInspectionMiddleware> logger,
            bool enabled
        ){
            _next = next;
            _logger = logger;
            _enabled = enabled;
        }
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private bool _enabled;

        public async Task Invoke(HttpContext context)
        {
            if (_enabled)
            {
                var sb = new StringBuilder($"Request Headers: {context.Request.Scheme}://{context.Request.Host} from {context.Connection.RemoteIpAddress}\n");
                sb.AppendLine($"\t{context.Request.Method} {context.Request.Path.Value} {context.Request.Protocol}");
                foreach (var header in context.Request.Headers)
                {
                    string val = header.Value;
                    if (header.Key.StartsWith("Authorization"))
                        val = header.Value.ToString().Split(' ').First() + " **redacted**";
                    sb.AppendLine($"\t{header.Key}: {val}");
                }
                _logger.LogInformation(sb.ToString());
            }
            await _next(context);
        }
    }
}
