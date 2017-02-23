using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using TopoMojo.Models;

namespace TopoMojo.Web
{
    public class JsonExceptionFilterAttribute : TypeFilterAttribute
    {
        public JsonExceptionFilterAttribute() : base(typeof(JsonExceptionFilter)) {}

        private class JsonExceptionFilter : IExceptionFilter
        {
            private readonly IHostingEnvironment _hostingEnvironment;
            private readonly ApplicationOptions _options;
            public JsonExceptionFilter(
                IHostingEnvironment hostingEnvironment,
                IOptions<ApplicationOptions> optionsAccessor)
            {
                _hostingEnvironment = hostingEnvironment;
                _options = optionsAccessor.Value;
            }

            public void OnException(ExceptionContext context)
            {
                JsonResult result = null;
                if (_hostingEnvironment.IsDevelopment() || _options.Site.ShowExceptionDetail)
                {
                    result = new JsonResult(context.Exception);
                }
                else
                {
                    result = new JsonResult(new {
                        Message = context.Exception.Message
                    });
                }
                result.StatusCode = 400;
                if (context.Exception is System.UnauthorizedAccessException) result.StatusCode = 401;
                context.Result = result;
            }
        }
    }
}