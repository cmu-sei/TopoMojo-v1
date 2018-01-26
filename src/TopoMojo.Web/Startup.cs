using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Jam.Accounts;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Data.EntityFrameworkCore;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web;
using TopoMojo.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using TopoMojo.Core.Abstractions;
using Swashbuckle.AspNetCore.Swagger;
using TopoMojo.Controllers;
using IdentityServer4.AccessTokenValidation;

namespace TopoMojo
{
    public class Startup
    {
         public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _authOptions = Configuration.GetSection("OpenIdConnect").Get<AuthorizationOptions>();
            _rootPath = env.ContentRootPath;
        }

        public IConfiguration Configuration { get; }
        public string _rootPath { get; set; }
        public AuthorizationOptions _authOptions = new AuthorizationOptions();


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions()

                .Configure<ControlOptions>(Configuration.GetSection("Control"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<ControlOptions>>().CurrentValue)

                .Configure<ClientSettings>(Configuration.GetSection("ClientSettings"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<ClientSettings>>().CurrentValue)

                .Configure<MessagingOptions>(Configuration.GetSection("Messaging"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<MessagingOptions>>().CurrentValue)

                .Configure<FileUploadOptions>(Configuration.GetSection("FileUpload"))
                .AddScoped(sp => sp.GetService<IOptionsMonitor<FileUploadOptions>>().CurrentValue);

            // add specified db provider
            services.AddDbProvider(Configuration);

            // add Account services
            services.AddJamAccounts()
                .WithConfiguration(() => Configuration.GetSection("Account"), opt => opt.RootPath = _rootPath)
                .WithDefaultRepository(builder => builder.UseConfiguredDatabase(Configuration))
                .WithProfileService(builder => builder.AddScoped<IProfileService, ProfileService>());

            // add TopoMojo
            services
                .AddTopoMojo(() => Configuration.GetSection("Core"))
                .AddTopoMojoData(builder => builder.UseConfiguredDatabase(Configuration));

            // Add framework services.
            services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new TextMediaTypeFormatter());
            });
            // .AddJsonOptions(options =>
            // {
            //     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            // });

            // services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IProfileResolver, ProfileResolver>();
            services.AddScoped<IFileUploadHandler, FileUploadHandler>();
            services.AddSingleton<IFileUploadMonitor, FileUploadMonitor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // add pod manager
            services.AddSingleton<IPodManager>(sp => {
                var options = Configuration.GetSection("Pod").Get<PodConfiguration>();
                return options.Type.ToLowerInvariant().Contains("vmock")
                    ? (IPodManager) new TopoMojo.vMock.PodManager(options, sp.GetService<ILoggerFactory>())
                    : (IPodManager) new TopoMojo.vSphere.PodManager(options, sp.GetService<ILoggerFactory>());
            });

            // string podManager = Configuration.GetSection("Pod:Type").Value ?? "";
            // if (podManager.ToLowerInvariant().Contains("vmock"))
            //     services.AddSingleton<IPodManager, TopoMojo.vMock.PodManager>();
            // else
            //     services.AddSingleton<IPodManager, TopoMojo.vSphere.PodManager>();

            // add signalr
            services.AddSignalR(options =>{});

            // services.AddSingleton(_ => new JsonSerializer
            //     {
            //         ContractResolver = new SignalRContractResolver(),
            //         ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //     }
            // );

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


            // add authentication
            AccountOptions accountOptions = Configuration.GetSection("Account").Get<AccountOptions>();
            TokenOptions tokenOptions = accountOptions.Token;

            services.AddAuthentication(
                options =>
                {
                    options.DefaultScheme = "local-jwt";
                    options.DefaultChallengeScheme = "local-jwt";
                }
            )
            .AddIdentityServerAuthentication(
                IdentityServerAuthenticationDefaults.AuthenticationScheme,
                options => {
                    options.Authority = _authOptions.Authority;
                    options.RequireHttpsMetadata = _authOptions.RequireHttpsMetadata;
                    options.ApiName = _authOptions.AuthorizationScope;
                }
            )
            .AddJwtBearer(
                tokenOptions.Scheme,
                options => {
                    options.RequireHttpsMetadata = _authOptions.RequireHttpsMetadata;
                    options.TokenValidationParameters =  new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenOptions.Key)),
                        ValidIssuer = tokenOptions.Issuer,
                        ValidAudience = tokenOptions.Audience,
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                }
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                if (!Configuration.GetValue<bool>("NoDevWebpack"))
                    app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                        HotModuleReplacement = true
                    });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            //move any querystring jwt to Auth bearer header
            app.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                {
                    if (context.Request.QueryString.HasValue)
                    {
                        string token = context.Request.QueryString.Value.Substring(1)
                            .Split('&')
                            .SingleOrDefault(x => x.Contains("bearer"))?.Split('=')[1];
                        if (!String.IsNullOrWhiteSpace(token))
                        {
                            context.Request.Headers.Add("Authorization", new[] {$"Bearer {token}"});
                        }
                    }
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

            app.UseAuthentication();

            app.UseSignalR(routes => {
                routes.MapHub<TopologyHub>("rem");
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
