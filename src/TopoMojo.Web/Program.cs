// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TopoMojo.Extensions;

namespace TopoMojo.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "TopoMojo";

            var hostBuilder = CreateHostBuilder(args)
                .Build()
                .InitializeDatabase();

            bool dbonly = args.ToList().Contains("--dbonly") || Environment.GetEnvironmentVariable("TOPOMOJO_DBONLY")?.ToLower() == "true";

            if (!dbonly)
                hostBuilder.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(opt => { })
                .UseStartup<Startup>();
            }
        );
    }
}
