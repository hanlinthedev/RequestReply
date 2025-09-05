
using QueueWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
       // Add the hosted service that initializes the static RabbitMqSvc
       services.AddHostedService<RabbitMqInitializer>();
    })
    .Build();

host.Run();