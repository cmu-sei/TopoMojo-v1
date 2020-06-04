// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TopoMojo.Web.Services;

namespace TopoMojo.Web.Extensions
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddIdentityResolver(
            this IServiceCollection services
        ) {
            return services
                .AddScoped<IdentityResolver>()
                .AddScoped<Abstractions.IIdentityResolver>(sp => sp.GetService<Services.IdentityResolver>())
                .AddScoped<IClaimsTransformation>(sp => sp.GetService<Services.IdentityResolver>())
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
