// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TopoMojo.Data;
using TopoMojo.Web.Models;

namespace TopoMojo.Web.Extensions
{
    public static class DatabaseExtensions
    {

        public static IHost InitializeDatabase(
            this IHost host
        )
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                IConfiguration config = services.GetRequiredService<IConfiguration>();
                IWebHostEnvironment env = services.GetService<IWebHostEnvironment>();
                DatabaseOptions options = config.GetSection("Database").Get<DatabaseOptions>()
                    ?? new DatabaseOptions();

                var dbContext = services.GetService<TopoMojoDbContext>();

                if (!dbContext.Database.IsInMemory())
                    dbContext.Database.Migrate();

                string seedFile = Path.Combine(env.ContentRootPath, options.SeedFile);

                if (File.Exists(seedFile)) {

                    DbSeedModel seedData = JsonSerializer.Deserialize<DbSeedModel>(
                        File.ReadAllText(seedFile)
                    );

                    foreach (var u in seedData.Users)
                    {
                        if (!dbContext.Users.Any(p => p.Id == u.GlobalId))
                        {
                            dbContext.Users.Add(new TopoMojo.Data.User
                            {
                                Name = u.Name,
                                Id = u.GlobalId,
                                WhenCreated = DateTime.UtcNow,
                                Role = TopoMojo.Models.UserRole.Administrator
                            });
                        }
                    }
                    dbContext.SaveChanges();
                }

                return host;
            }
        }
    }
}
