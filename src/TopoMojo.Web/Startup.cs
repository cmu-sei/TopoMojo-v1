using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings-custom.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string dbProvider = Configuration.GetSection("Database:Provider").Value ?? "";
            if (dbProvider.ToLowerInvariant() == "sqlserver")
            {
                services.AddDbContext<TopoMojoDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"),
                    b => b.MigrationsAssembly("TopoMojo.Web")));

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("Identity.SqlServer")));
            }
            else
            {
                services.AddDbContext<TopoMojoDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("SqliteConnection"),
                    b => b.MigrationsAssembly("TopoMojo.Web")));

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("Identity.Sqlite")));
            }

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOptions()
                .Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"))
                //.Configure<IdentityOptions>(Configuration.GetSection("Options:Identity"))
                .Configure<TopoMojo.Models.PodConfiguration>(Configuration.GetSection("ApplicationOptions:Pod"));
                // .Configure<SiteConfiguration>(Configuration.GetSection("ApplicationOptions:SiteConfiguration"))
                // .Configure<FileUploadConfiguration>(Configuration.GetSection("ApplicationOptions:FileUploadConfiguration"));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserResolver, UserResolver>();

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>   {
                config.Cookies.ApplicationCookie.AutomaticChallenge = false;
                // config.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents
                // {
                //     OnRedirectToLogin = ctx =>
                //     {
                //         if (ctx.Request.Path.StartsWithSegments("/api")) //&&
                //             //ctx.Response.StatusCode == (int) HttpStatusCode.OK)
                //         {
                //             ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                //         }
                //         else
                //         {
                //             ctx.Response.Redirect(ctx.RedirectUri);
                //         }
                //         return Task.FromResult(0);
                //     }
                // };
            });
            // Add framework services.
            services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new TextMediaTypeFormatter());
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IFileUploadMonitor, FileUploadMonitor>();

            services.AddScoped<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();
            //services.AddScoped<SimulationManager, SimulationManager>();
            services.AddScoped<TopologyManager, TopologyManager>();
            services.AddScoped<TemplateManager, TemplateManager>();
            services.AddScoped<InstanceManager, InstanceManager>();

            string podManager = Configuration.GetSection("ApplicationOptions:Site:PodManagerType").Value ?? "";
            if (podManager.ToLowerInvariant().Contains("vmock"))
                services.AddSingleton<IPodManager, TopoMojo.vMock.PodManager>();
            else
                services.AddSingleton<IPodManager, TopoMojo.vSphere.PodManager>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
            app.UseIdentity();

            Models.IdentityOptions io = new Models.IdentityOptions();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(io.Authentication.TokenKey));
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = io.Authentication.TokenIssuer,
                    ValidateAudience = true,
                    ValidAudience = io.Authentication.TokenAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
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

            if (env.IsDevelopment())
            {
                bool recreate = Convert.ToBoolean(Configuration["Database:DevModeRecreate"]);
                app.InitializeDatabase(Convert.ToBoolean(Configuration["Database:AutoMigrate"]), recreate);
            }
            else
            {
                app.InitializeDatabase(Convert.ToBoolean(Configuration["Database:AutoMigrate"]), false);
            }
        }
    }

    public static class DatabaseStartupExtensions
    {
        public static void InitializeDatabase(this IApplicationBuilder app, bool autoMigrate, bool recreate)
        {
            Person admin = null;

            TopoMojoDbContext ctx = app.ApplicationServices.GetService<TopoMojoDbContext>();
            if (ctx != null)
            {
                if (recreate)
                    ctx.Database.EnsureDeleted();

                if (autoMigrate)
                    ctx.Database.Migrate();
                else
                    ctx.Database.EnsureCreated();

                admin = ctx.People.FirstOrDefault();
                if (admin == null)
                {
                    admin = new Person
                    {
                        Name = "Administrator",
                        GlobalId = Guid.NewGuid().ToString(),
                        WhenCreated = DateTime.UtcNow,
                        IsAdmin = true
                    };
                    ctx.People.Add(admin);
                    ctx.SaveChanges();
                }


            }

            ApplicationDbContext appdb = app.ApplicationServices.GetService<ApplicationDbContext>();
            if (appdb != null)
            {
                if (recreate)
                    appdb.Database.EnsureDeleted();

                appdb.Database.EnsureCreated();
                if (!appdb.Users.Any() && admin != null)
                {
                    UserManager<ApplicationUser> mgr = app.ApplicationServices.GetRequiredService<UserManager<ApplicationUser>>();

                    string email = "admin@this.ws";
                    ApplicationUser user = new ApplicationUser {
                        Id = admin.GlobalId,
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        IsAdmin = true,
                        PersonId = admin.Id
                    };
                    var result = mgr.CreateAsync(user, "321ChangeMe!").Result;

                }
            }
        }
    }

}
