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

            BuildWebHost(args)
            .InitializeDatabase()
            .Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
