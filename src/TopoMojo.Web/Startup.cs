using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using TopoMojo.Abstractions;
using TopoMojo.Controllers;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;
//using TopoMojo.Web;

namespace TopoMojo.Web
{
    public class Startup
    {
         public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options =>
            {
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
                .AddSingleton<IPodManager>(sp => {
                    var options = Configuration.GetSection("Pod").Get<PodConfiguration>();
                    return String.IsNullOrWhiteSpace(options.Url)
                        ? (IPodManager) new TopoMojo.vMock.PodManager(options, sp.GetService<ILoggerFactory>())
                        : (IPodManager) new TopoMojo.vSphere.PodManager(options, sp.GetService<ILoggerFactory>());
                });

            #endregion

            #region Configure Signalr

            services.AddSignalR(options =>{});
            services.AddSingleton<HubCache>();

            #endregion

            #region Configure Swagger
            //add swagger
            services.AddSwaggerGen(options =>
            {
                // if (System.IO.File.Exists(xmlFileName))
                // {
                //     options.IncludeXmlComments(xmlFileName);
                // }

                options.SwaggerDoc("v1", new Info
                {
                    Title = "TopoMojo",
                    Version = "v1",
                    Description = "API documentation and interaction for " + "TopoMojo"
                });

                // options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                // {
                //     Type = "oauth2",
                //     Flow = "implicit",
                //     AuthorizationUrl = _authOptions.AuthorizationUrl,
                //     Scopes = new Dictionary<string, string>
                //     {
                //         { _authOptions.AuthorizationScope, "public api access" }
                //     }
                // });
                options.DescribeAllEnumsAsStrings();
                options.CustomSchemaIds(x => x.FullName);
            });
            #endregion

            #region Configure Authentication

            var oidcOptions = Configuration.GetSection("OpenIdConnect").Get<AuthorizationOptions>();
            services.AddAuthentication(
                options =>
                {
                    options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                }
            )
            .AddIdentityServerAuthentication(
                IdentityServerAuthenticationDefaults.AuthenticationScheme,
                options => {
                    options.Authority = oidcOptions.Authority;
                    options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                    options.ApiName = oidcOptions.AuthorizationScope;
                }
            );

            #endregion
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            if (env.IsDevelopment() || Configuration.GetValue<bool>("Control:ShowExceptionDetail"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            if (env.IsDevelopment())
            {
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                    HotModuleReplacement = true
                });
            }

            app.UseCors("default");
            app.UseStaticFiles();

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
                        context.Request.Headers.Add("Authorization", new[] {$"Bearer {token}"});
                }

                await next.Invoke();

            });

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "docs/{documentName}/api.json";
            });
            // app.UseSwaggerUI(c =>
            // {
            //     c.RoutePrefix = "docs";
            //     c.SwaggerEndpoint("/docs/v1/api.json", "TopoMojo" + " (v1)");
            //     c.ConfigureOAuth2(_authOptions.ClientId, _authOptions.ClientSecret, _authOptions.ClientId, _authOptions.ClientName);
            //     c.InjectStylesheet("/css/site.css");
            //     c.InjectOnCompleteJavaScript("/js/custom-swag.js");
            // });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            app.UseAuthentication();

            app.UseSignalR(routes => {
                routes.MapHub<TopologyHub>("/hub");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name:"console",
                    template: "Console/{id}/{name?}");

                routes.MapRoute(
                    name: "uploads",
                    template: "File/{action=Index}/{id}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });

        }
    }



}
