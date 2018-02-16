using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Web
{
    public class JsonExceptionFilterAttribute : TypeFilterAttribute
    {
        public JsonExceptionFilterAttribute() : base(typeof(JsonExceptionFilter)) {}

        private class JsonExceptionFilter : IExceptionFilter
        {
            private readonly IHostingEnvironment _hostingEnvironment;
            private readonly ControlOptions _options;
            public JsonExceptionFilter(
                IHostingEnvironment hostingEnvironment,
                ControlOptions options)
            {
                _hostingEnvironment = hostingEnvironment;
                _options = options;
            }

            public void OnException(ExceptionContext context)
            {
                Exception exception = null;

                if (_hostingEnvironment.IsDevelopment() || _options.ShowExceptionDetail)
                {
                    exception = context.Exception;
                }
                else
                {
                    string ex = context.Exception
                        .GetType().Name
                        .Replace("Exception", "")
                        .ToUpper();

                    exception = new Exception($"EXCEPTION.{ex} {context.Exception.Message}");
                }

                context.Result = new JsonResult(new JsonException(exception))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            private class JsonException
            {
                public string Message { get; set; }
                public string StackTrace { get; set; }
                public List<string> InnerExceptionMessages { get; set; } = new List<string>();

                public JsonException(Exception exception)
                {
                    this.Message = exception.Message;
                    this.StackTrace = exception.StackTrace;
                    AddInnerException(exception);
                }
                private void AddInnerException(Exception exception)
                {
                    Exception ex = exception.InnerException;
                    if (ex != null)
                    {
                        InnerExceptionMessages.Add(ex.Message);
                        AddInnerException(ex);
                    }
                }
            }
        }
    }
}