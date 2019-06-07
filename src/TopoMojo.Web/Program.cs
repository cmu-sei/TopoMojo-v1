// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using TopoMojo.Extensions;

namespace TopoMojo.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "TopoMojo";

            CreateWebHostBuilder(args)
                .Build()
                .InitializeDatabase()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
