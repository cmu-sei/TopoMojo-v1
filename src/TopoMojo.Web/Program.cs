using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Step.Common;

namespace TopoMojo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            JsonAppSettings.Merge(path, "appsettings.json", "appsettings-custom.json");
            if (args.Length > 0 && args[0].ToLower() == "merge")
                return;

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5001")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
