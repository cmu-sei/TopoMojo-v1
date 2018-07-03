using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using IdentityModel;
using Jam.Accounts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using TopoMojo.Abstractions;
using TopoMojo.Controllers;
using TopoMojo.Core.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web;

namespace TopoMojo
{
    public class Startup
    {
         public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; }

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
                .WithConfiguration(() => Configuration.GetSection("Account"), opt => opt.RootPath = Env.ContentRootPath)
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
            //     //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            // });

            services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IProfileResolver, ProfileResolver>();
            services.AddScoped<IFileUploadHandler, FileUploadHandler>();
            services.AddSingleton<IFileUploadMonitor, FileUploadMonitor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<HubCache>();

            // add pod manager
            services.AddSingleton<IPodManager>(sp => {
                var options = Configuration.GetSection("Pod").Get<PodConfiguration>();
                return String.IsNullOrWhiteSpace(options.Url)
                    ? (IPodManager) new TopoMojo.vMock.PodManager(options, sp.GetService<ILoggerFactory>())
                    : (IPodManager) new TopoMojo.vSphere.PodManager(options, sp.GetService<ILoggerFactory>());
            });

            #region Configure Signalr

            services.AddSignalR(options =>{});

            // services.AddSingleton(_ => new JsonSerializer
            //     {
            //         ContractResolver = new SignalRContractResolver(),
            //         ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //     }
            // );
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
            // add authentication
            AccountOptions accountOptions = Configuration.GetSection("Account").Get<AccountOptions>();
            TokenOptions tokenOptions = accountOptions.Token;

            var authBuilder = services.AddAuthentication(
                options =>
                {
                    options.DefaultScheme = tokenOptions.Scheme;
                    options.DefaultChallengeScheme = tokenOptions.Scheme;
                }
            )
            .AddJwtBearer(
                tokenOptions.Scheme,
                options => {
                    options.RequireHttpsMetadata = false; //tokenOptions.RequireHttpsMetadata;
                    options.TokenValidationParameters =  new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenOptions.Key)),
                        ValidIssuer = tokenOptions.Issuer,
                        ValidAudience = tokenOptions.Audience,
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role
                    };
                }
            );

            var oidcOptions = Configuration.GetSection("OpenIdConnect").Get<AuthorizationOptions>();
            if (oidcOptions.Enabled)
            {
                authBuilder.AddIdentityServerAuthentication(
                    "IdSrv", //IdentityServerAuthenticationDefaults.AuthenticationScheme,
                    options => {
                        options.Authority = oidcOptions.Authority;
                        options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
                        options.ApiName = oidcOptions.AuthorizationScope;
                    }
                );
            }
            else
            {
                authBuilder.AddJwtBearer("IdSrv", options => {});
            }
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
                routes.MapHub<TopologyHub>("hub");
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
