// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
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

            Environment = env;

            Settings = Configuration.Get<AppSettings>() ?? new AppSettings();

            // Oidc = Configuration.GetSection("Authorization").Get<AuthorizationOptions>()
            //     ?? new AuthorizationOptions();

            // Branding = Configuration.GetSection("Branding").Get<BrandingOptions>()
            //     ?? new BrandingOptions();

            // CacheOptions = Configuration.GetSection("Cache").Get<CacheOptions>()
            //     ?? new CacheOptions();

            Settings.Cache.SharedFolder = Path.Combine(
                env.ContentRootPath,
                Settings.Cache.SharedFolder ?? ""
            );


            // Headers = Configuration.GetSection("Headers").Get<HeaderOptions>()
            //     ?? new HeaderOptions();

            // Database = Configuration.GetSection("Database").Get<DatabaseOptions>()
            //     ?? new DatabaseOptions();

            // FileUploadOptions = Configuration.GetSection("FileUpload").Get<FileUploadOptions>()
            //     ?? new FileUploadOptions();

            // PodOptions = Configuration.GetSection("Pod").Get<TopoMojo.Models.HypervisorServiceConfiguration>()
            //     ?? new TopoMojo.Models.HypervisorServiceConfiguration();

            // ApiKeyClients = Configuration.GetSection("ApiKeyClients").Get<List<ApiKeyClient>>()
            //     ?? new List<ApiKeyClient>();

            // TopoMojoOptions = Configuration.GetSection("Core").Get<CoreOptions>()
            //     ?? new CoreOptions();

            if (env.IsDevelopment())
            {
                Settings.Oidc.RequireHttpsMetadata = false;
            }
        }

        public IHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        AppSettings Settings { get; }
        // public AuthorizationOptions Oidc { get; }
        // private BrandingOptions Branding { get; }
        // private CacheOptions CacheOptions { get; }
        // private DatabaseOptions Database { get; }
        // private HeaderOptions Headers { get; }
        // private FileUploadOptions FileUploadOptions { get; }
        // private TopoMojo.Models.HypervisorServiceConfiguration PodOptions { get; }
        // private List<ApiKeyClient> ApiKeyClients { get; }
        // private CoreOptions TopoMojoOptions { get; }

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

            services.ConfigureForwarding(Settings.Headers.Forwarding);

            services.AddCors(
                opt => opt.AddPolicy(
                    Settings.Headers.Cors.Name,
                    Settings.Headers.Cors.Build()
                )
            );

            if (Settings.OpenApi.Enabled)
                services.AddSwagger(Settings.Oidc, Settings.OpenApi);

            services.AddCache(() => Settings.Cache);

            services.AddDataProtection()
                .SetApplicationName(AppConstants.DataProtectionPurpose)
                .PersistKeys(() => Settings.Cache);

            services.AddSignalRHub();

            services.AddFileUpload(Settings.FileUpload);

            if (Environment.IsDevelopment().Equals(false))
                services.AddHostedService<ScheduledTasksService>();

            // Configure TopoMojo
            services
                .AddIdentityResolver()
                .AddTopoMojo(Settings.Core)
                .AddTopoMojoData(Settings.Database.Provider, Settings.Database.ConnectionString)
                .AddTopoMojoHypervisor(() => Settings.Pod)
                .AddSingleton<AutoMapper.IMapper>(
                    new AutoMapper.MapperConfiguration(cfg =>
                    {
                        cfg.AddTopoMojoMaps();
                    }).CreateMapper()
                );

            // Configure Auth
            services.AddConfiguredAuthentication(Settings.Oidc);
            services.AddConfiguredAuthorization();

        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseJsonExceptions();

            if (!string.IsNullOrEmpty(Settings.PathBase))
                app.UsePathBase(Settings.PathBase);

            if (Settings.Headers.LogHeaders)
                app.UseHeaderInspection();

            if (!string.IsNullOrEmpty(Settings.Headers.Forwarding.TargetHeaders))
                app.UseForwardedHeaders();

            if (Settings.Headers.UseHsts)
                app.UseHsts();

            app.UseRouting();

            app.UseCors(Settings.Headers.Cors.Name);

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            if (Settings.OpenApi.Enabled)
                app.UseConfiguredSwagger(Settings.OpenApi, Settings.Oidc.Audience, Settings.PathBase);

            app.UseEndpoints(ep =>
            {
                ep.MapHub<Hubs.AppHub>("/hub").RequireAuthorization();

                ep.MapControllers().RequireAuthorization();
            });
        }
    }
}
