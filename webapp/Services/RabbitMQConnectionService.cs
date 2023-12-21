using RabbitMQ.Client;

namespace webapp.Services;

public class RabbitMQConnectionService
{
    public IConnection Connection { get; }
    
    public RabbitMQConnectionService()
    {
        var rabbitmqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER");
        var rabbitmqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
        var rabbitmqUrl = Environment.GetEnvironmentVariable("RABBITMQ_URL");
        var factory = new ConnectionFactory
        {
            HostName = rabbitmqUrl,
            UserName = rabbitmqUser,
            Password = rabbitmqPassword,
        };
        
        const int retryInterval = 2000; // Retry every 2 seconds
        const int maxRetryDuration = 30000; // Max duration 30 seconds
        int timeElapsed = 0;

        while (timeElapsed < maxRetryDuration)
        {
            try
            {
                Connection = factory.CreateConnection();
                break;
            }
            catch
            {
                Thread.Sleep(retryInterval);
                timeElapsed += retryInterval;
            }
        }

        if (Connection == null)
        {
            throw new Exception("Failed to connect to RabbitMQ within the specified timeout period.");
        }
    }
}
