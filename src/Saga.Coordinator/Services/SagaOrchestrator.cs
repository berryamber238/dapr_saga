using Dapr.Client;
using DaprDemo.Shared.Models;
using DaprDemo.Shared.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Saga.Coordinator.Configuration;
using Serilog;

namespace Saga.Coordinator.Services;

public interface ISagaOrchestrator
{
    Task<string> InitSagaAsync(TransactionRequest request);
    Task HandleSagaEventAsync(SagaEvent evt);
    Task<SagaTransaction?> GetSagaStateAsync(string transactionId);
}

public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly DaprClient _daprClient;
    private readonly SagaTransactionRepository _repository;
    private readonly RetryOptions _retryOptions;
    
    // Service App IDs for Dapr Service Invocation
    private const string APP_CTA = "service-cta";
    private const string APP_GENESIS = "service-genesis";
    private const string APP_PERFECTCAGE = "service-perfectcage";

    public SagaOrchestrator(DaprClient daprClient, SagaTransactionRepository repository, IOptions<RetryOptions> retryOptions)
    {
        _daprClient = daprClient;
        _repository = repository;
        _retryOptions = retryOptions.Value;
    }

    public async Task<string> InitSagaAsync(TransactionRequest request)
    {
        // Use provided ID if available, otherwise generate new
        var txId = string.IsNullOrEmpty(request.TransactionId) ? Guid.NewGuid().ToString() : request.TransactionId;
        Log.Information($"[Coordinator] Initializing Saga {txId}");

        // 1. Save Initial State to MongoDB
        var saga = new SagaTransaction
        {
            TransactionId = txId,
            Status = "Pending",
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        await _repository.CreateAsync(saga);

        // 2. Parallel Invocation
        var txRequest = new { TransactionId = txId, request.BusinessId, request.Payload };

        var t1 = InvokeServiceAsync(APP_CTA, "api/cta/transaction", txRequest);
        var t2 = InvokeServiceAsync(APP_GENESIS, "api/genesis/transaction", txRequest);
        var t3 = InvokeServiceAsync(APP_PERFECTCAGE, "api/perfectcage/transaction", txRequest);

        var results = await Task.WhenAll(t1, t2, t3);

        // 3. Check for immediate failures (e.g. 500 errors, network issues)
        if (results.Any(x => !x))
        {
             Log.Error($"[Coordinator] One or more services failed to accept the transaction. Triggering compensation for {txId}");
             
             // Update status
             saga.Status = "Compensating";
             saga.UpdateTime = DateTime.UtcNow;
             await _repository.UpdateAsync(x => x.Id == saga.Id, saga);

             // Trigger Compensation
             await TriggerCompensationAsync(saga);
        }

        return txId;
    }

    private async Task<bool> InvokeServiceAsync(string appId, string methodName, object data)
    {
        int maxRetries = _retryOptions.MaxRetries;
        int delayMilliseconds = _retryOptions.InitialDelayMilliseconds;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await _daprClient.InvokeMethodAsync(HttpMethod.Post, appId, methodName, data);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Coordinator] Failed to invoke {appId}/{methodName}. Attempt {i + 1}/{maxRetries}.");
                
                if (i == maxRetries - 1)
                {
                    Log.Error(ex, $"[Coordinator] Failed to invoke {appId}/{methodName} after {maxRetries} attempts.");
                    return false;
                }

                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2; // Exponential backoff: 1s, 2s, 4s
            }
        }
        return false;
    }

    public async Task HandleSagaEventAsync(SagaEvent evt)
    {
        Log.Information($"[Coordinator] Received event: {evt.ServiceName} - {evt.Status} for {evt.TransactionId}");

        var saga = await _repository.GetByTransactionIdAsync(evt.TransactionId);
        if (saga == null)
        {
            Log.Warning($"[Coordinator] Saga state not found for {evt.TransactionId}");
            return;
        }

        if (evt.Status == "Failed")
        {
            if (!saga.FailedServices.Contains(evt.ServiceName))
            {
                saga.FailedServices.Add(evt.ServiceName);
            }
        }
        else if (evt.Status == "Success")
        {
            if (!saga.CompletedServices.Contains(evt.ServiceName))
            {
                saga.CompletedServices.Add(evt.ServiceName);
            }
        }

        // Logic to determine if we should update status
        // This is a simplified logic. Real world would need more robust state machine.
        // We rely on the event to tell us what happened.
        
        if (saga.FailedServices.Any() && saga.Status != "Compensating" && saga.Status != "Compensated")
        {
             saga.Status = "Compensating";
             await TriggerCompensationAsync(saga);
        }
        else if (saga.CompletedServices.Count >= 3 && !saga.FailedServices.Any() && saga.Status != "Completed")
        {
            // All 3 services (CTA, Genesis, PerfectCage) succeeded
            saga.Status = "Completed";
            Log.Information($"[Coordinator] Saga {saga.TransactionId} completed successfully.");
        }
        
        saga.UpdateTime = DateTime.UtcNow;
        await _repository.UpdateAsync(x => x.Id == saga.Id, saga);
    }

    private async Task TriggerCompensationAsync(SagaTransaction saga)
    {
        var txRequest = new { TransactionId = saga.TransactionId };
        
        // In a real scenario, we'd know which ones succeeded to only compensate those.
        // Here we just broadcast compensate to all for simplicity or safety.
        // Also using InvokeServiceAsync to benefit from retry policy during compensation
        await InvokeServiceAsync(APP_CTA, "api/cta/compensate", txRequest);
        await InvokeServiceAsync(APP_GENESIS, "api/genesis/compensate", txRequest);
        await InvokeServiceAsync(APP_PERFECTCAGE, "api/perfectcage/compensate", txRequest);
    }

    public async Task<SagaTransaction?> GetSagaStateAsync(string transactionId)
    {
        return await _repository.GetByTransactionIdAsync(transactionId);
    }
}

public class TransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
}

public class SagaEvent
{
    public string TransactionId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
