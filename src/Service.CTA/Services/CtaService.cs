using Dapr.Client;
using DaprSaga.Shared.Models;
using DaprSaga.Shared.Repositories;
using MongoDB.Driver;
using Serilog;

namespace Service.CTA.Services;

public interface ICtaService
{
    Task<bool> ProcessTransactionAsync(string transactionId, string businessId, object payload);
    Task<bool> CompensateTransactionAsync(string transactionId);
}

public class CtaService : ICtaService
{
    private readonly DaprClient _daprClient;
    private readonly BusinessDataRepository _businessRepo;
    private readonly CompensateLogRepository _compensateRepo;
    private readonly IMongoClient _mongoClient;
    private readonly EventStoreRepository _eventStore;
    private const string PUBSUB_NAME = "pubsub";
    private const string TOPIC_NAME = "saga-status";
    private const string SERVICE_NAME = "Service.CTA";

    public CtaService(DaprClient daprClient, BusinessDataRepository businessRepo, CompensateLogRepository compensateRepo, IMongoClient mongoClient, EventStoreRepository eventStore)
    {
        _daprClient = daprClient;
        _businessRepo = businessRepo;
        _compensateRepo = compensateRepo;
        _mongoClient = mongoClient;
        _eventStore = eventStore;
    }

    public async Task<bool> ProcessTransactionAsync(string transactionId, string businessId, object payload)
    {
        Log.Information($"[CTA] Processing transaction {transactionId}");
        using var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await Task.Delay(500); // Simulate work

            // Fix for MongoDB serialization of JsonElement
            object dataPayload = payload;
            if (payload is System.Text.Json.JsonElement jsonElement)
            {
                dataPayload = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<MongoDB.Bson.BsonDocument>(jsonElement.GetRawText());
            }

            // Save Business Data
            var data = new BusinessData
            {
                TransactionId = transactionId,
                BusinessId = businessId,
                ServiceName = SERVICE_NAME,
                Data = dataPayload,
                Status = "Success",
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow
            };
            await _businessRepo.CreateAsync(session, data);

            // Save to Outbox
            await SaveOutboxEventAsync(session, transactionId, "Success", "CTA processed");

            await session.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            Log.Error(ex, $"[CTA] Error processing {transactionId}");
            // Optional: Try to save failure event in a separate transaction
            return false;
        }
    }

    public async Task<bool> CompensateTransactionAsync(string transactionId)
    {
        Log.Information($"[CTA] Compensating transaction {transactionId}");
        using var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await Task.Delay(300);

            // Log Compensation
            var log = new CompensateLog
            {
                TransactionId = transactionId,
                ServiceName = SERVICE_NAME,
                CompensateTime = DateTime.UtcNow,
                Result = "Success",
                Message = "Compensated successfully"
            };
            await _compensateRepo.CreateAsync(session, log);

            // Update Business Data Status
            var data = await _businessRepo.GetAsync(x => x.TransactionId == transactionId && x.ServiceName == SERVICE_NAME);
            if (data != null)
            {
                data.Status = "Compensated";
                data.UpdateTime = DateTime.UtcNow;
                await _businessRepo.UpdateAsync(session, x => x.Id == data.Id, data);
            }

            // Save to Outbox
            await SaveOutboxEventAsync(session, transactionId, "Compensated", "CTA compensated");

            await session.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            Log.Error(ex, $"[CTA] Error compensating {transactionId}");
            return false;
        }
    }

    private async Task SaveOutboxEventAsync(IClientSessionHandle session, string txId, string status, string message)
    {
        var evt = new 
        {
            TransactionId = txId,
            ServiceName = SERVICE_NAME,
            Status = status,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        var outbox = new OutboxMessage
        {
            Topic = TOPIC_NAME,
            Payload = evt,
            Status = "Pending",
            CreateTime = DateTime.UtcNow
        };

        await _eventStore.CreateOutboxAsync(session, outbox);
    }
}
