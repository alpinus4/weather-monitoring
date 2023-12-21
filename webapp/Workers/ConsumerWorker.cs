
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using MongoDB.Driver;
using webapp.Services;
using System.Threading.Channels;
using MongoDB.Driver.Core.Bindings;
using webapp.Model;

namespace webapp.Workers;


public class ConsumerWorker : BackgroundService
{
    private readonly IConnection _rabbitMQConnection;
    private readonly MongoClient _mongoDBConnection;
    public ConsumerWorker(RabbitMQConnectionService rabbitMQConnectionService, MongoDBConnectionService mongoDBConnectionService)
    {
        _rabbitMQConnection = rabbitMQConnectionService.Connection;
        _mongoDBConnection = mongoDBConnectionService.Connection;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var channel = _rabbitMQConnection.CreateModel())
        {

            channel.QueueDeclare("sensor_data", false, false, false, null);
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, eventArguments) =>
            {
                var msg = Encoding.UTF8.GetString(eventArguments.Body.ToArray());


                Data? data = JsonSerializer.Deserialize<Data>(msg);

                if (data == null) return;


                _mongoDBConnection.GetDatabase("db").GetCollection<Data>("sensor_data").InsertOne(data);


            };

            channel.BasicConsume("sensor_data", true, consumer);



            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }

            channel.Close();
            consumer.Model = null;

        }

    }
}