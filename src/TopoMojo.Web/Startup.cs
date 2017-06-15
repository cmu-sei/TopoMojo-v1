using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Step.Accounts;
using Step.Common;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Data;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web;

namespace TopoMojo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            _rootPath = env.ContentRootPath;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings-custom.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        public string _rootPath { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddOptions()
                .Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<ApplicationOptions>>().Value)

                .Configure<CoreOptions>(Configuration.GetSection("ApplicationOptions:Core"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<CoreOptions>>().Value)

                .Configure<PodConfiguration>(Configuration.GetSection("ApplicationOptions:Pod"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<PodConfiguration>>().Value)

                .Configure<ClientAuthenticationSettings>(Configuration.GetSection("ClientAuthenticationSettings"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<ClientAuthenticationSettings>>().Value)

                .Configure<BrandingOptions>(Configuration.GetSection("Branding"))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<BrandingOptions>>().Value);

            // add Account services
            services.Configure<AccountOptions>(Configuration.GetSection("Account"))
                .AddDbContext<AccountDbContext>(builder => builder.UseConfiguredDatabase(Configuration))
                .AddScoped(sp => sp.GetService<IOptionsSnapshot<AccountOptions>>().Value)
                .AddScoped<IAccountManager<Account>, AccountManager<Account>>();

            // add X509IssuerStore
            services.Configure<X509IssuerStoreOptions>(options => { options.RootPath = _rootPath; })
                .AddScoped(sp => sp.GetService<IOptions<X509IssuerStoreOptions>>().Value)
                .AddSingleton<Step.Accounts.IX509IssuerStore, Step.Accounts.X509IssuerStore>();

            // add TokenService
            services.Configure<Step.Accounts.TokenOptions>(Configuration.GetSection("Account:Token"))
                .AddScoped(sp => sp.GetService<IOptions<Step.Accounts.TokenOptions>>().Value)
                .AddScoped<Step.Accounts.ITokenService, Step.Accounts.DefaultTokenService>();

            // add ProfileResolver and ProfileService
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddScoped<IProfileResolver, ProfileResolver>()
                .AddScoped<IProfileService, ProfileService>();

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
            services.AddDbContext<TopoMojoDbContext>(builder => builder.UseConfiguredDatabase(Configuration));

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IFileUploadMonitor, FileUploadMonitor>();

            services.AddScoped<TopologyManager, TopologyManager>();
            services.AddScoped<TemplateManager, TemplateManager>();
            services.AddScoped<InstanceManager, InstanceManager>();
            services.AddScoped<ProfileManager, ProfileManager>();

            string podManager = Configuration.GetSection("ApplicationOptions:Site:PodManagerType").Value ?? "";
            if (podManager.ToLowerInvariant().Contains("vmock"))
                services.AddSingleton<IPodManager, TopoMojo.vMock.PodManager>();
            else
                services.AddSingleton<IPodManager, TopoMojo.vSphere.PodManager>();

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
                            .SingleOrDefault(x => x.Contains("jwt"))?.Split('=')[1];
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
            AuthorizationOptions authOptions = Configuration.GetSection("Authorization").Get<AuthorizationOptions>();
            if (!String.IsNullOrWhiteSpace(authOptions.Authority))
            {
                app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = authOptions.Authority,
                    AllowedScopes = { authOptions.AuthorizationScope },
                    RequireHttpsMetadata = authOptions.RequireHttpsMetadata,
                });
            }

            //handle local login bearer tokens
            Models.IdentityOptions io = new Models.IdentityOptions();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(io.Authentication.TokenKey));
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role",
                    IssuerSigningKey = signingKey,
                    ValidIssuer = io.Authentication.TokenIssuer,
                    ValidAudience = io.Authentication.TokenAudience
                }
            });

            app.UseMvc(routes =>
            {
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

            app.InitializeDatabase(Configuration, env.IsDevelopment());

            // if (env.IsDevelopment())
            // {
            //     bool recreate = Convert.ToBoolean(Configuration["Database:DevModeRecreate"]);
            //     app.InitializeDatabase(Convert.ToBoolean(Configuration["Database:AutoMigrate"]), recreate);
            // }
            // else
            // {
            //     app.InitializeDatabase(Convert.ToBoolean(Configuration["Database:AutoMigrate"]), false);
            // }
        }
    }

    public static class DatabaseStartupExtensions
    {
        public static DbContextOptionsBuilder UseConfiguredDatabase(
            this DbContextOptionsBuilder builder,
            IConfigurationRoot config
        )
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string dbProvider = config["Database:Provider"];
            var connectionString = config.GetConnectionString(dbProvider);

            switch (dbProvider)
            {
                case "Sqlite":
                builder.UseSqlite(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
                break;

                case "SqlServer":
                builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
                break;
            }
            return builder;
        }

        // public static void InitializeDatabase(this IApplicationBuilder app, bool autoMigrate, bool recreate)
        // {
        public static IApplicationBuilder InitializeDatabase(this IApplicationBuilder app, IConfigurationRoot config, bool isDevelopment)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                DatabaseOptions options = config.GetSection("Database").Get<DatabaseOptions>();
                TopoMojoDbContext db = scope.ServiceProvider.GetRequiredService<TopoMojoDbContext>();
                AccountDbContext accountsDb = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
                if (isDevelopment && options.DevModeRecreate)
                    db.Database.EnsureDeleted();

                db.Database.Migrate();
                accountsDb.Database.Migrate();
                //Account user = mgr.FindByGuidAsync("9fd3c38e-58b0-4af1-80d1-1895af91f1f9").Result;

                string guid = Guid.NewGuid().ToString();
                if (!accountsDb.Accounts.Any())
                {
                    IAccountManager<Account> mgr = app.ApplicationServices.GetRequiredService<IAccountManager<Account>>();
                    mgr.RegisterWithCredentialsAsync(
                        new Credentials {
                            Username = "admin@this.ws",
                            Password = "321ChangeMe!"
                        }, guid).Wait();
                }

                if (!db.People.Any())
                {
                    db.People.Add(new Person {
                        GlobalId = guid,
                        Name = "Administrator",
                        WhenCreated = DateTime.UtcNow,
                        IsAdmin = true
                    });
                    db.SaveChanges();
                }
            }

            return app;

            // ApplicationDbContext appdb = app.ApplicationServices.GetService<ApplicationDbContext>();
            // if (appdb != null)
            // {
            //     if (recreate)
            //         appdb.Database.EnsureDeleted();

            //     appdb.Database.EnsureCreated();
            //     if (!appdb.Users.Any() && admin != null)
            //     {
            //         UserManager<ApplicationUser> mgr = app.ApplicationServices.GetRequiredService<UserManager<ApplicationUser>>();

            //         string email = "admin@this.ws";
            //         ApplicationUser user = new ApplicationUser {
            //             Id = admin.GlobalId,
            //             UserName = email,
            //             Email = email,
            //             EmailConfirmed = true,
            //             IsAdmin = true,
            //             PersonId = admin.Id
            //         };
            //         var result = mgr.CreateAsync(user, "321ChangeMe!").Result;

            //     }
            // }
        }
    }

}
