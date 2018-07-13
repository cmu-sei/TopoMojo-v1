using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using TopoMojo.Extensions;

namespace TopoMojo
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
