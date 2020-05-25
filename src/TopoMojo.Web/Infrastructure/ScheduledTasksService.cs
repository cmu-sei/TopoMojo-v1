using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TopoMojo.Services;

namespace TopoMojo.Web.Services
{
    public class ScheduledTasksService : IHostedService
    {
        private Timer _timer;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public ScheduledTasksService(
            IServiceProvider serviceProvider,
            ILogger<ScheduledTasksService> logger
        )
        {
            _services = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var rand = new Random();

            _timer = new Timer(StaleCheck,
                null,
                rand.Next(10, 30) * 60 * 1000,
                rand.Next(50, 70) * 60 * 1000
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();

            return Task.CompletedTask;
        }

        private void StaleCheck(object state)
        {
           using (var scope = _services.CreateScope())
           {
               var janitor = scope.ServiceProvider.GetService<JanitorService>();
               janitor.Cleanup().Wait();
           }
        }
    }
}
