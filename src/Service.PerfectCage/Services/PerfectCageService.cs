using Dapr.Client;
using DaprSaga.Shared.Models;
using DaprSaga.Shared.Repositories;
using Serilog;

namespace Service.PerfectCage.Services;

public interface IPerfectCageService
{
    Task<bool> ProcessTransactionAsync(string transactionId, string businessId, object payload);
    Task<bool> CompensateTransactionAsync(string transactionId);
}

public class PerfectCageService : IPerfectCageService
{
    private readonly DaprClient _daprClient;
    private readonly BusinessDataRepository _businessRepo;
    private readonly CompensateLogRepository _compensateRepo;
    private const string PUBSUB_NAME = "pubsub";
    private const string TOPIC_NAME = "saga-status";
    private const string SERVICE_NAME = "Service.PerfectCage";

    public PerfectCageService(DaprClient daprClient, BusinessDataRepository businessRepo, CompensateLogRepository compensateRepo)
    {
        _daprClient = daprClient;
        _businessRepo = businessRepo;
        _compensateRepo = compensateRepo;
    }

    public async Task<bool> ProcessTransactionAsync(string transactionId, string businessId, object payload)
    {
        Log.Information($"[PerfectCage] Processing transaction {transactionId}");
        try
        {
            await Task.Delay(400); // Simulate work

            // Fix for MongoDB serialization of JsonElement
            object dataPayload = payload;
            if (payload is System.Text.Json.JsonElement jsonElement)
            {
                dataPayload = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<MongoDB.Bson.BsonDocument>(jsonElement.GetRawText());
            }

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
            await _businessRepo.CreateAsync(data);

            await PublishEventAsync(transactionId, "Success", "PerfectCage processed");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[PerfectCage] Error processing {transactionId}");
            await PublishEventAsync(transactionId, "Failed", ex.Message);
            return false;
        }
    }

    public async Task<bool> CompensateTransactionAsync(string transactionId)
    {
        Log.Information($"[PerfectCage] Compensating transaction {transactionId}");
        try
        {
            await Task.Delay(300);

            var log = new CompensateLog
            {
                TransactionId = transactionId,
                ServiceName = SERVICE_NAME,
                CompensateTime = DateTime.UtcNow,
                Result = "Success",
                Message = "Compensated successfully"
            };
            await _compensateRepo.CreateAsync(log);

            var data = await _businessRepo.GetAsync(x => x.TransactionId == transactionId && x.ServiceName == SERVICE_NAME);
            if (data != null)
            {
                data.Status = "Compensated";
                data.UpdateTime = DateTime.UtcNow;
                await _businessRepo.UpdateAsync(x => x.Id == data.Id, data);
            }

            await PublishEventAsync(transactionId, "Compensated", "PerfectCage compensated");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[PerfectCage] Error compensating {transactionId}");
            return false;
        }
    }

    private async Task PublishEventAsync(string txId, string status, string message)
    {
        var evt = new 
        {
            TransactionId = txId,
            ServiceName = SERVICE_NAME,
            Status = status,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
        await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, evt);
    }
}
