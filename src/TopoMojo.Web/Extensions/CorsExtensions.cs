// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace TopoMojo.Extensions
{
    public static class CorsPolicyExtensions
    {
        public static CorsOptions UseConfiguredCors(
            this CorsOptions builder,
            IConfiguration section
        )
        {
            CorsPolicyOptions policy = section?.Get<CorsPolicyOptions>() ?? new CorsPolicyOptions();
            builder.AddPolicy("default", policy.Build());
            return builder;
        }
    }

    public class CorsPolicyOptions
    {
        public string[] Origins { get; set; } = new string[]{};
        public string[] Methods { get; set; } = new string[]{};
        public string[] Headers { get; set; } = new string[]{};
        public bool AllowCredentials { get; set; } = true;

        public CorsPolicy Build()
        {
            CorsPolicyBuilder policy = new CorsPolicyBuilder();
            if (Origins.IsEmpty()) policy.AllowAnyOrigin(); else policy.WithOrigins(Origins);
            if (Methods.IsEmpty()) policy.AllowAnyMethod(); else policy.WithMethods(Origins);
            if (Headers.IsEmpty()) policy.AllowAnyHeader(); else policy.WithHeaders(Origins);
            if (AllowCredentials) policy.AllowCredentials(); else policy.DisallowCredentials();
            return policy.Build();
        }
    }
}
