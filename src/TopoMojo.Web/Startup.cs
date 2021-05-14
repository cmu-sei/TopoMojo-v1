// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TopoMojo.Web.Controllers;
using TopoMojo.Web.Extensions;
using TopoMojo.Web.Services;

namespace TopoMojo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;

            Oidc = Configuration.GetSection("Authorization").Get<AuthorizationOptions>()
                ?? new AuthorizationOptions();

            Branding = Configuration.GetSection("Branding").Get<BrandingOptions>()
                ?? new BrandingOptions();

            CacheOptions = Configuration.GetSection("Cache").Get<CacheOptions>()
                ?? new CacheOptions();

            CacheOptions.SharedFolder = Path.Combine(
                env.ContentRootPath,
                CacheOptions.SharedFolder ?? ""
            );

            Headers = Configuration.GetSection("Headers").Get<HeaderOptions>()
                ?? new HeaderOptions();

            Database = Configuration.GetSection("Database").Get<DatabaseOptions>()
                ?? new DatabaseOptions();

            FileUploadOptions = Configuration.GetSection("FileUpload").Get<FileUploadOptions>()
                ?? new FileUploadOptions();

            PodOptions = Configuration.GetSection("Pod").Get<TopoMojo.Models.HypervisorServiceConfiguration>()
                ?? new TopoMojo.Models.HypervisorServiceConfiguration();

            ApiKeyClients = Configuration.GetSection("ApiKeyClients").Get<List<ApiKeyClient>>()
                ?? new List<ApiKeyClient>();

            TopoMojoOptions = Configuration.GetSection("Core").Get<CoreOptions>()
                ?? new CoreOptions();

            if (env.IsDevelopment())
            {
                Oidc.RequireHttpsMetadata = false;
            }
        }

        public IConfiguration Configuration { get; }
        public AuthorizationOptions Oidc { get; }
        private BrandingOptions Branding { get; }
        private CacheOptions CacheOptions { get; }
        private DatabaseOptions Database { get; }
        private HeaderOptions Headers { get; }
        private FileUploadOptions FileUploadOptions { get; }
        private TopoMojo.Models.HypervisorServiceConfiguration PodOptions { get; }
        private List<ApiKeyClient> ApiKeyClients { get; }
        private CoreOptions TopoMojoOptions { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
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

            services.ConfigureForwarding(Headers.Forwarding);

            services.AddCors(
                opt => opt.AddPolicy(
                    Headers.Cors.Name,
                    Headers.Cors.Build()
                )
            );

            if (Branding.IncludeSwagger)
                services.AddSwagger(Oidc, Branding);

            services.AddCache(() => CacheOptions);

            services.AddDataProtection()
                .SetApplicationName(AppConstants.DataProtectionPurpose)
                .PersistKeys(() => CacheOptions);

            services.AddSignalR(options => {});
            services.AddSingleton<HubCache>();
            services.AddSingleton<IUserIdProvider, SubjectProvider>();

            services.AddFileUpload(FileUploadOptions);

            services.AddHostedService<ScheduledTasksService>();

            #region Configure TopoMojo

            services
                .AddIdentityResolver()
                .AddTopoMojo(TopoMojoOptions)
                .AddTopoMojoData(Database.Provider, Database.ConnectionString)
                .AddTopoMojoHypervisor(() => PodOptions)
                .AddSingleton<AutoMapper.IMapper>(
                    new AutoMapper.MapperConfiguration(cfg =>
                    {
                        cfg.AddTopoMojoMaps();
                    }).CreateMapper()
                );

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
                    options.Audience = Oidc.Audience;
                    options.Authority = Oidc.Authority;
                    options.RequireHttpsMetadata = Oidc.RequireHttpsMetadata;
                })
                .AddApiKey(ApiKeyAuthentication.AuthenticationScheme, options =>
                {
                    options.Clients = ApiKeyClients;
                })
                .AddTicketAuthentication(TicketAuthentication.AuthenticationScheme, options => {})
                .AddCookie(AppConstants.CookieScheme, opt =>
                {
                    // opt.ExpireTimeSpan = new TimeSpan(4, 0, 0);
                    // opt.SlidingExpiration = true;
                    opt.Cookie = new CookieBuilder
                    {
                        Name = AppConstants.CookieScheme
                    };
                })
                ;

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
                    .RequireClaim("role", TopoMojo.Models.UserRole.Administrator.ToString())
                    .Build());

                _.AddPolicy("TrustedClients", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(ApiKeyAuthentication.AuthenticationScheme)
                    .Build());

                _.AddPolicy("OneTimeTicket", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(TicketAuthentication.AuthenticationScheme)
                    .Build());

                _.AddPolicy("Players", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme,
                        AppConstants.CookieScheme
                    )
                    .Build());
                _.AddPolicy("TicketOrCookie", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        AppConstants.CookieScheme,
                        TicketAuthentication.AuthenticationScheme
                    )
                    .Build());
            });

            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseJsonExceptions();

            if (!string.IsNullOrEmpty(Branding.PathBase))
                app.UsePathBase(Branding.PathBase);

            if (Headers.LogHeaders)
                app.UseHeaderInspection();

            if (!string.IsNullOrEmpty(Headers.Forwarding.TargetHeaders))
                app.UseForwardedHeaders();

            if (Headers.UseHsts)
                app.UseHsts();

            app.UseRouting();

            app.UseCors(Headers.Cors.Name);

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            if (Branding.IncludeSwagger)
                app.UseConfiguredSwagger(Oidc, Branding);

            app.UseEndpoints(ep =>
            {
                ep.MapHub<AppHub>("/hub");

                ep.MapControllers().RequireAuthorization();
            });
        }
    }
}
