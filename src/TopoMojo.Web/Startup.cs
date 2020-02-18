// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TopoMojo.Abstractions;
using TopoMojo.Controllers;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;

            _appName = Configuration["Control:ApplicationName"] ?? "TopoMojo";

            _engineApiKey = Configuration["Core:EngineKey"] ?? Guid.NewGuid().ToString();

            oidcOptions = Configuration.GetSection("Authorization").Get<AuthorizationOptions>();

            if (env.IsDevelopment())
            {
                oidcOptions.RequireHttpsMetadata = false;
            }
        }

        private string _engineApiKey;
        private string _appName;
        public IConfiguration Configuration { get; }
        public AuthorizationOptions oidcOptions { get; set; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(
                options =>
                {
                    options.InputFormatters.Insert(0, new TextMediaTypeFormatter());
                })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                );
            });

            services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("Cors")));

            services.AddDbProvider(() => Configuration.GetSection("Database"));

            #region Configure TopoMojo

            services
                .AddApplicationOptions(Configuration)
                .AddIdentityResolver()
                .AddTopoMojo(() => Configuration.GetSection("Core"))
                .AddTopoMojoData(builder => builder.UseConfiguredDatabase(Configuration))
                .AddScoped<IFileUploadHandler, FileUploadHandler>()
                .AddSingleton<IFileUploadMonitor, FileUploadMonitor>()
                .AddSingleton<AutoMapper.IMapper>(
                    new AutoMapper.MapperConfiguration(cfg => {
                        cfg.AddTopoMojoMaps();
                    }).CreateMapper()
                )
                .AddSingleton<IHypervisorService>(sp =>
                {
                    var options = Configuration.GetSection("Pod").Get<TopoMojo.Models.HypervisorServiceConfiguration>();
                    return String.IsNullOrWhiteSpace(options.Url)
                        ? (IHypervisorService)new TopoMojo.Services.MockHypervisorService(options, sp.GetService<ILoggerFactory>())
                        : (IHypervisorService)new TopoMojo.vSphere.HypervisorService(options, sp.GetService<ILoggerFactory>());
                })
                .AddHostedService<ServiceHostWrapper<IHypervisorService>>();

            // services.AddAutoMapper();

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
                    options.Audience = oidcOptions.Audience;
                    options.Authority = oidcOptions.Authority;
                    options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role",
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            var token = ctx.Request.Query["access_token"];
                            if (!String.IsNullOrEmpty(token))
                            {
                                ctx.Token = token;
                            }
                            return System.Threading.Tasks.Task.CompletedTask;
                        },
                        OnTokenValidated = ctx =>
                        {
                            // TODO // MAYBE // ensure local user registration
                            return System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });

            #endregion

            #region Configure Swagger
            //add swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TopoMojo",
                    Version = "v1",
                    Description = "API documentation and interaction"
                });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(oidcOptions.SwaggerClient.AuthorizationUrl),
                            Scopes = new Dictionary<string, string>
                            {
                                { oidcOptions.Audience, "User Access" }
                            }
                        }
                    },
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { oidcOptions.Audience }
                    }
                });
                // options.DescribeAllEnumsAsStrings();
            });
            #endregion

        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseCors("default");
            app.UseStaticFiles();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api";
                c.SwaggerEndpoint("/api/v1/swagger.json", Configuration["Control:ApplicationName"] ?? _appName + " (v1)");
                c.OAuthClientId(oidcOptions.SwaggerClient?.ClientId);
                c.OAuthAppName(oidcOptions.SwaggerClient?.ClientName ?? oidcOptions.SwaggerClient?.ClientId);
            });

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.StartsWith("/api/engine"))
                {
                    if (context.Request.Headers["X-API-KEY"] != _engineApiKey)
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }
                }
                await next();
            });

            app.UseAuthorization();

            app.UseEndpoints(ep =>
            {
                ep.MapHub<TopologyHub>("/hub");
                ep.MapControllers();
            });
        }
    }
}
