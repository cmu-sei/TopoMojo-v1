// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace TopoMojo.Web
{
    public class JsonExceptionFilterAttribute : TypeFilterAttribute
    {
        public JsonExceptionFilterAttribute() : base(typeof(JsonExceptionFilter)) {}

        private class JsonExceptionFilter : IExceptionFilter
        {
            private readonly IHostEnvironment _hostingEnvironment;
            private readonly ControlOptions _options;
            public JsonExceptionFilter(
                IHostEnvironment hostingEnvironment,
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
