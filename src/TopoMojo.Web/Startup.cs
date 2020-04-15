// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TopoMojo.Abstractions;
using TopoMojo.Controllers;
using TopoMojo.Extensions;
using TopoMojo.Services;

namespace TopoMojo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;

            _appName = Configuration["Control:ApplicationName"] ?? "TopoMojo";

            oidcOptions = Configuration.GetSection("Authorization").Get<AuthorizationOptions>();

            if (env.IsDevelopment())
            {
                oidcOptions.RequireHttpsMetadata = false;
            }
        }

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
                .AddTopoMojoHypervisor(() =>
                    Configuration.GetSection("Pod").Get<TopoMojo.Models.HypervisorServiceConfiguration>()
                )
                .AddScoped<IFileUploadHandler, FileUploadHandler>()
                .AddSingleton<IFileUploadMonitor, FileUploadMonitor>()
                .AddSingleton<AutoMapper.IMapper>(
                    new AutoMapper.MapperConfiguration(cfg =>
                    {
                        cfg.AddTopoMojoMaps();
                    }).CreateMapper()
                );

            #endregion

            #region Configure Signalr

            services.AddSignalR(options => { });
            services.AddSingleton<HubCache>();

            #endregion

            #region Configure Authentication
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Audience = oidcOptions.Audience;
                    options.Authority = oidcOptions.Authority;
                    options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                })
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                    ApiKeyAuthentication.AuthenticationScheme,
                    opt => opt.Clients = Configuration.GetSection("ApiKeyClients").Get<List<ApiKeyClient>>()
                )
                .AddScheme<TicketAuthenticationOptions, TicketAuthenticationHandler>(
                    TicketAuthentication.AuthenticationScheme,
                    opt => {}
                );

            services.AddAuthorization(_ =>
            {
                _.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme
                    ).Build();

                _.AddPolicy("AdminOnly", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme
                    )
                    .RequireClaim("role", "administrator")
                    .Build());

                _.AddPolicy("TrustedClients", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(ApiKeyAuthentication.AuthenticationScheme)
                    .Build());

                _.AddPolicy("OneTimeTicket", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(TicketAuthentication.AuthenticationScheme)
                    .Build());
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
                c.RouteTemplate = "api/{documentName}/openapi.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api";
                c.SwaggerEndpoint("/api/v1/openapi.json", Configuration["Control:ApplicationName"] ?? _appName + " (v1)");
                c.OAuthClientId(oidcOptions.SwaggerClient?.ClientId);
                c.OAuthAppName(oidcOptions.SwaggerClient?.ClientName ?? oidcOptions.SwaggerClient?.ClientId);
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Cookies.ContainsKey("Authorization")
                    && !context.Request.Headers.ContainsKey("Authorization")
                )
                {
                    context.Request.Headers.Add("Authorization", context.Request.Cookies["Authorization"]);
                }
                await next.Invoke();
            });

            // app.UseMiddleware<CrossSiteRequestMiddleware>();

            // app.UseMiddleware<BearerCookieMiddleware>(oidcOptions.BearerCookieEndpoint);

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(ep =>
            {
                ep.MapHub<TopologyHub>("/hub");
                ep.MapControllers().RequireAuthorization();
            });
        }
    }
}
