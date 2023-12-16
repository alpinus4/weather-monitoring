
using RabbitMQ.Client;
using WebApp.Services;
namespace WebApp.Workers;
public class ConsumerWorker : BackgroundService
{
    private readonly IConnection _rabbitMQConnection;
    public ConsumerWorker(RabbitMQConnectionService rabbitMQConnectionService)
    {
        _rabbitMQConnection = rabbitMQConnectionService.Connection;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var channel = _rabbitMQConnection.CreateModel())
        {
            await Task.Delay(1000); // Delays for 1 second (1000 milliseconds)
            // TODO read from rabbit and save to database, show to user
        }
    }
}