
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using QueueWorker;

namespace QueueWorker;

public class RabbitMqInitializer : IHostedService
{
   private readonly IConfiguration _config;

   public RabbitMqInitializer(IConfiguration config)
   {
      _config = config;
   }

   public async Task StartAsync(CancellationToken cancellationToken)
   {
      // Initialize the static service
      await RabbitMqSvc.ConnectRm(_config);
   }

   public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}