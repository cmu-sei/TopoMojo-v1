// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TopoMojo.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationOptions(
            this IServiceCollection services,
            IConfiguration config
        ) {
              return services.AddOptions()

                .Configure<ControlOptions>(config.GetSection("Control"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<ControlOptions>>().CurrentValue)

                .Configure<FileUploadOptions>(config.GetSection("FileUpload"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<FileUploadOptions>>().CurrentValue);

        }

        public static IServiceCollection AddProfileResolver(
            this IServiceCollection services
        ) {
            return services.AddMemoryCache()
                .AddScoped<Services.ProfileResolver>()
                .AddScoped<Abstractions.IProfileResolver>(sp => sp.GetService<Services.ProfileResolver>())
                .AddScoped<IClaimsTransformation>(sp => sp.GetService<Services.ProfileResolver>())
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        }
    }
}
