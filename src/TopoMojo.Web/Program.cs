using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TopoMojo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("hosting.json", optional: true)
                .AddCommandLine(args)
                .Build();

            string path = Directory.GetCurrentDirectory();
            if (config["just"]?.ToLower() == "merge")
            {
                JsonAppSettings.Merge(path, "appsettings.json", "appsettings-custom.json");
                return;
            }

            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5004")
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            Console.Title = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
            host.Run();
        }
    }
}
