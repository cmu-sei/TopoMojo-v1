// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using TopoMojo.Abstractions;
using TopoMojo.Controllers;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            oidcOptions = Configuration.GetSection("OpenIdConnect").Get<AuthorizationOptions>();
        }

        public IConfiguration Configuration { get; }
        public AuthorizationOptions oidcOptions { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                var global = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

                //options.Filters.Add(new AuthorizeFilter(global));
                options.InputFormatters.Insert(0, new TextMediaTypeFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));
            services.AddDbProvider(() => Configuration.GetSection("Database"));

            #region Configure TopoMojo

            services
                .AddApplicationOptions(Configuration)
                .AddProfileResolver()
                .AddTopoMojo(() => Configuration.GetSection("Core"))
                .AddTopoMojoData(builder => builder.UseConfiguredDatabase(Configuration))
                .AddScoped<IFileUploadHandler, FileUploadHandler>()
                .AddSingleton<IFileUploadMonitor, FileUploadMonitor>()
                .AddSingleton<IPodManager>(sp =>
                {
                    var options = Configuration.GetSection("Pod").Get<PodConfiguration>();
                    return String.IsNullOrWhiteSpace(options.Url)
                        ? (IPodManager)new TopoMojo.vMock.PodManager(options, sp.GetService<ILoggerFactory>())
                        : (IPodManager)new TopoMojo.vSphere.PodManager(options, sp.GetService<ILoggerFactory>());
                })
                .AddHostedService<ServiceHostWrapper<IPodManager>>();

            #endregion

            #region Configure Signalr

            services.AddSignalR(options => { });
            services.AddSingleton<HubCache>();

            #endregion

            #region Configure Authentication
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = oidcOptions.Authority;
                    options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                    options.Audience = oidcOptions.AuthorizationScope;
                    options.SaveToken = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            #endregion

            #region Configure Swagger
            //add swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "TopoMojo",
                    Version = "v1",
                    Description = "API documentation and interaction"
                });

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = oidcOptions.AuthorizationUrl,
                    Scopes = new Dictionary<string, string>
                    {
                        { oidcOptions.AuthorizationScope, "public api access" }
                    }
                });
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", new[] { oidcOptions.AuthorizationScope } }
                });
                options.DescribeAllEnumsAsStrings();
            });
            #endregion

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("default");

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.Context.Request.Path.Value.ToLower() == "/index.html")
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
                    }
                }
            });

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api";
                c.SwaggerEndpoint("/api/v1/swagger.json", Configuration["Control:ApplicationName"]??"TopoMojo" + " (v1)");
                c.OAuthClientId(oidcOptions.SwaggerClient?.ClientId);
                c.OAuthAppName(oidcOptions.SwaggerClient?.ClientName);
                c.OAuthClientSecret(oidcOptions.SwaggerClient?.ClientSecret);
            });

            app.UseQuerystringBearerToken();
            app.UseAuthentication();
            app.UseSignalR(routes =>
            {
                routes.MapHub<TopologyHub>("/hub");
            });
            app.UseMvc();
        }
    }
}
