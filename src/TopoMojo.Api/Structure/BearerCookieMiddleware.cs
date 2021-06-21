// Copyright 2021 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public class BearerCookieMiddleware
    {
        public BearerCookieMiddleware(
            RequestDelegate next,
            IHostEnvironment environment,
            string endpoint
        )
        {
            _next = next;
            _endpoint = endpoint;
            _dev = environment.IsDevelopment();
            _cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            };

            _random = new Random();
            _hash = SHA256.Create();
        }

        private const string HEADER = "Authorization";
        private readonly RequestDelegate _next;
        private string _endpoint;
        private bool _dev;
        private CookieOptions _cookieOptions;
        private Random _random;
        private HashAlgorithm _hash;

        public async Task Invoke(HttpContext context)
        {
            if (
                !context.Request.Headers.ContainsKey(HEADER)
                && context.Request.Cookies.ContainsKey(HEADER)
            )
            {
                context.Request.Headers.Add(HEADER, context.Request.Cookies[HEADER]);
            }

            if (
                !string.IsNullOrWhiteSpace(_endpoint)
                && _endpoint.StartsWith(context.Request.Method)
                && _endpoint.EndsWith(context.Request.Path)
            )
            {
                context.Response.Cookies.Append(
                    HEADER,
                    context.Request.Headers[HEADER],
                    _cookieOptions
                );

                if (!_dev || true)
                {
                    byte[] salt = new byte[18];
                    _random.NextBytes(salt);

                    byte[] token = Encoding.UTF8.GetBytes(context.Request.Headers[HEADER]);

                    byte[] hash = _hash.ComputeHash(salt.Concat(token).ToArray());

                    string result = $"{Convert.ToBase64String(salt)}.{BitConverter.ToString(hash)}";

                    context.Response.Cookies.Append("XSRF-TOKEN", result.Replace("-","").ToLower());
                }
            }

            await _next(context);

        }
    }
}
