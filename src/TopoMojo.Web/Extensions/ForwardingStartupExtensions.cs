// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using TopoMojo.Web;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ForwardingStartupExtensions
    {

        public static IServiceCollection ConfigureForwarding(
            this IServiceCollection services,
            ForwardHeaderOptions options
        )
        {
            services.Configure<ForwardedHeadersOptions>(config => {

                if (Enum.TryParse<ForwardedHeaders>(
                    options.TargetHeaders ?? "None",
                    true,
                    out ForwardedHeaders targets)
                )
                {
                    config.ForwardedHeaders = targets;
                }

                config.ForwardLimit = options.ForwardLimit;

                if (options.ForwardLimit == 0)
                {
                    config.ForwardLimit = null;
                }

                string nets = options.KnownNetworks;
                if (string.IsNullOrEmpty(nets))
                    nets = "10.0.0.0/8 172.16.0.0/12 192.168.0.0/24 ::ffff:a00:0/104 ::ffff:b00a:0/108 ::ffff:c0d0:0/120";

                foreach (string item in nets.Split(new char[] { ' ', ','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] net = item.Split('/');

                    if (IPAddress.TryParse(net.First(), out IPAddress ipaddr)
                        && Int32.TryParse(net.Last(), out int prefix)
                    )
                    {
                        config.KnownNetworks.Add(new IPNetwork(ipaddr, prefix));
                    }
                }

                if (!string.IsNullOrEmpty(options.KnownProxies))
                {
                    foreach (string ip in options.KnownProxies.Split(new char[] { ' ', ','}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (IPAddress.TryParse(ip, out IPAddress ipaddr))
                        {
                            config.KnownProxies.Add(ipaddr);
                        }
                    }
                }

            });

            return services;
        }
    }
}
