using RequestReply.Services;

public class RabbitMqInitializer : IHostedService
{
    private readonly IConfiguration _config;

    public RabbitMqInitializer(IConfiguration config)
    {
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await RabbitMqSvc.ConnectRm(_config);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}