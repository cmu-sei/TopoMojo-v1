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
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

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
                });

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
            // services.AddAuthentication(
            //     options =>
            //     {
            //         options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
            //         options.DefaultChallengeScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
            //     }
            // )
            // .AddIdentityServerAuthentication(
            //     IdentityServerAuthenticationDefaults.AuthenticationScheme,
            //     options => {
            //         options.Authority = oidcOptions.Authority;
            //         options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
            //         options.ApiName = oidcOptions.AuthorizationScope;
            //     }
            // );

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
                    { "oauth2", new[] { "readAccess", "writeAccess" } }
                });
                options.DescribeAllEnumsAsStrings();
                //options.CustomSchemaIds(x => x.FullName);
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

            //move any querystring jwt to Auth bearer header
            app.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"])
                    && context.Request.QueryString.HasValue)
                {
                    string token = context.Request.QueryString.Value
                        .Substring(1)
                        .Split('&')
                        .SingleOrDefault(x => x.StartsWith("bearer="))?.Split('=')[1];

                    if (!String.IsNullOrWhiteSpace(token))
                        context.Request.Headers.Add("Authorization", new[] { $"Bearer {token}" });
                }

                await next.Invoke();

            });

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "openapi/{documentName}/api.json";
            });
            // app.UseSwaggerUI(c =>
            // {
            //     c.RoutePrefix = "openapi";
            //     c.SwaggerEndpoint("/openapi/v1/api.json", "TopoMojo" + " (v1)");
            //     c.OAuthClientId(oidcOptions.ClientId);
            //     c.OAuthClientSecret(oidcOptions.ClientSecret);
            //     c.OAuthAppName(oidcOptions.ClientName);
            //     c.InjectStylesheet("/css/site.css");
            //     c.InjectJavascript("/js/custom-swag.js");
            // });

            app.UseAuthentication();

            app.UseSignalR(routes =>
            {
                routes.MapHub<TopologyHub>("/hub");
            });

            app.UseMvc();

            // Default route return client app
            app.Run(async (context) =>
            {
                string indexFile = Path.Combine(env.WebRootPath, "index.html");
                var fileInfo = new System.IO.FileInfo(indexFile);
                context.Response.StatusCode = 200;
                context.Response.ContentLength = fileInfo.Length;
                context.Response.Headers.Append("Content-Type", "text/html");

                using (FileStream fs = File.OpenRead(indexFile))
                {
                    await fs.CopyToAsync(context.Response.Body);
                }
            });

        }
    }
}
