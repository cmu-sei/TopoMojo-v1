using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Jam.Accounts;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Data;
using TopoMojo.Core.Entities;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web;
using TopoMojo.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;

namespace TopoMojo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            _rootPath = env.ContentRootPath;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings-custom.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            DbOptions = Configuration.GetSection("Database").Get<DatabaseOptions>();
            DbOptions.MigrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
        }

        public IConfigurationRoot Configuration { get; }
        public string _rootPath { get; set; }
        public DatabaseOptions DbOptions { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions()
                .Configure<ControlOptions>(Configuration.GetSection("Control"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<ControlOptions>>().Value)

                .Configure<CoreOptions>(Configuration.GetSection("Core"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<CoreOptions>>().Value)

                .Configure<PodConfiguration>(Configuration.GetSection("Pod"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<PodConfiguration>>().Value)

                .Configure<ClientSettings>(Configuration.GetSection("ClientSettings"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<ClientSettings>>().Value)

                .Configure<MessagingOptions>(Configuration.GetSection("Messaging"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<MessagingOptions>>().Value)

                .Configure<FileUploadOptions>(Configuration.GetSection("FileUpload"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<FileUploadOptions>>().Value);

            // add Account services
            services.AddJamAccounts()
            .WithConfiguration(() => Configuration.GetSection("Account"), opt => opt.RootPath = _rootPath)
            .WithDefaultRepository(builder => builder.UseConfiguredDatabase(DbOptions))
            .WithProfileService(builder => builder.AddScoped<IProfileService, ProfileService>());


            // Add framework services.
            services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new TextMediaTypeFormatter());
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            // services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));
            services.AddDbContext<TopoMojoDbContext>(builder => builder.UseConfiguredDatabase(DbOptions));
            services.AddScoped<TopologyManager, TopologyManager>();
            services.AddScoped<TemplateManager, TemplateManager>();
            services.AddScoped<GamespaceManager, GamespaceManager>();
            services.AddScoped<ProfileManager, ProfileManager>();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IProfileResolver, ProfileResolver>();
            services.AddSingleton<IFileUploadMonitor, FileUploadMonitor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            string podManager = Configuration.GetSection("Pod:Type").Value ?? "";
            if (podManager.ToLowerInvariant().Contains("vmock"))
                services.AddSingleton<IPodManager, TopoMojo.vMock.PodManager>();
            else
                services.AddSingleton<IPodManager, TopoMojo.vSphere.PodManager>();

            services.AddSignalR(options =>
                options.Hubs.EnableDetailedErrors = true);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            //app.UseIdentityServer();

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

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // handle IdentityServer bearer tokens
            IdentityServerAuthenticationOptions authOptions = Configuration.GetSection("OpenIdConnect").Get<IdentityServerAuthenticationOptions>();
            authOptions.JwtBearerEvents = new JwtBearerEvents
            {
                OnAuthenticationFailed = (context) => {
                    Console.WriteLine(context.Exception.Message);
                    context.SkipToNextMiddleware();
                    return Task.FromResult(0);
                }
            };

            if (!String.IsNullOrWhiteSpace(authOptions.Authority))
            {
                app.UseIdentityServerAuthentication(authOptions);
            }

            //handle local login bearer tokens
            AccountOptions accountOptions = app.ApplicationServices.GetService<AccountOptions>();
            TokenOptions tokenOptions = accountOptions.Token;
            //TokenOptions tokenOptions = Configuration.GetSection("Account:Token").Get<TokenOptions>();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenOptions.Key));
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AuthenticationScheme = tokenOptions.Scheme,
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role",
                    IssuerSigningKey = signingKey,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience
                },

            });

            app.UseSignalR();

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

            app.InitializeDatabase(
                DbOptions,
                env.IsDevelopment()
            );

        }
    }



}
