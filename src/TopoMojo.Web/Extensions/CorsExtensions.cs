// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

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
        public bool AllowAnyOrigin { get; set; }
        public bool AllowAnyMethod { get; set; }
        public bool AllowAnyHeader { get; set; }
        public bool SupportsCredentials { get; set; }

        public CorsPolicy Build()
        {
            CorsPolicyBuilder policy = new CorsPolicyBuilder();
            if (this.AllowAnyOrigin)
                policy.AllowAnyOrigin();
            else
                policy.WithOrigins(this.Origins);

            if (this.AllowAnyHeader)
                policy.AllowAnyHeader();
            else
                policy.WithHeaders(this.Headers);

            if (this.AllowAnyMethod)
                policy.AllowAnyMethod();
            else
                policy.WithMethods(this.Methods);

            if (this.SupportsCredentials)
                policy.AllowCredentials();
            else
                policy.DisallowCredentials();

            return policy.Build();
        }
    }
}