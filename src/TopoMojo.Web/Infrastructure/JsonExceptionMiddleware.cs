// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TopoMojo.Web;

namespace TopoMojo.Web
{
    public class JsonExceptionMiddleware
    {
        public JsonExceptionMiddleware(
            RequestDelegate next
        )
        {
            _next = next;
        }
        private readonly RequestDelegate _next;

        public async Task Invoke(HttpContext context)
        {
            try {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 500;
                    string message = "Error";
                    Type type = ex.GetType();

                    if (
                        ex is System.InvalidOperationException
                        || type.Namespace.StartsWith("TopoMojo")
                    ) {
                        context.Response.StatusCode = 400;
                        // message = ex.Message;
                        message = type.Name
                            .Split('.')
                            .Last()
                            .Replace("Exception", "");

                        // message += $" {ex.Message}";
                    }

                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = message }));
                }
            }

        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    public static class JsonExceptionStartupExtensions
    {
        public static IApplicationBuilder UseJsonExceptions (
            this IApplicationBuilder builder
        )
        {
            return builder.UseMiddleware<JsonExceptionMiddleware>();
        }
    }
}
