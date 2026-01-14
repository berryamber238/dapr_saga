using Dapr.Client;
using DaprDemo.Shared.Models;
using MongoDB.Driver;

namespace Service.OutboxWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DaprClient _daprClient;
    private readonly IMongoClient _mongoClient;
    private readonly IMongoCollection<OutboxMessage> _outboxCollection;

    public Worker(ILogger<Worker> logger, DaprClient daprClient, IMongoClient mongoClient, IConfiguration config)
    {
        _logger = logger;
        _daprClient = daprClient;
        _mongoClient = mongoClient;
        var database = _mongoClient.GetDatabase(config["MongoDB:DatabaseName"]);
        _outboxCollection = database.GetCollection<OutboxMessage>("Outbox");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for Dapr sidecar to be ready
        await _daprClient.WaitForSidecarAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var filter = Builders<OutboxMessage>.Filter.Eq(x => x.Status, "Pending");
                var messages = await _outboxCollection.Find(filter).Limit(10).ToListAsync(stoppingToken);

                foreach (var msg in messages)
                {
                    try
                    {
                        // Use PubSub to send to RabbitMQ
                        // Topic is defined in the message
                        await _daprClient.PublishEventAsync("pubsub", msg.Topic, msg.Payload, cancellationToken: stoppingToken);

                        // Update status
                        var update = Builders<OutboxMessage>.Update
                            .Set(x => x.Status, "Sent")
                            .Set(x => x.SentTime, DateTime.UtcNow);
                        
                        await _outboxCollection.UpdateOneAsync(x => x.Id == msg.Id, update, cancellationToken: stoppingToken);
                        
                        _logger.LogInformation("Sent message {Id}", msg.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send message {Id}", msg.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling outbox");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
