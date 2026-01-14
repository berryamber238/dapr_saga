using Dapr.Client;
using DaprSaga.Shared.Models;
using Microsoft.Extensions.Options;
using Saga.Coordinator.Configuration;
using DaprSaga.Shared.Repositories;
using Saga.Coordinator.Models;
using Serilog;
using System.Text.Json;
using Nacos.V2; // Added
using System.Net.Http.Json; // Added

namespace Saga.Coordinator.Services;

public class CashOutSagaOrchestrator
{
    private readonly DaprClient _daprClient;
    private readonly SagaTransactionRepository _repository;
    private readonly RetryOptions _retryOptions;
    private readonly INacosNamingService _nacosNamingService;
    private readonly HttpClient _httpClient;

    public CashOutSagaOrchestrator(DaprClient daprClient, SagaTransactionRepository repository, IOptions<RetryOptions> retryOptions, INacosNamingService nacosNamingService, IHttpClientFactory httpClientFactory)
    {
        _daprClient = daprClient;
        _repository = repository;
        _retryOptions = retryOptions.Value;
        _nacosNamingService = nacosNamingService;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task InitSagaAsync(SharedTransactionRequest request)
    {
        var txId = request.TransactionId;
        Log.Information($"[CashOutSaga] Initializing {txId}");

        var services = new List<string> { "service-cta" };
        
        try 
        {
            string json = JsonSerializer.Serialize(request.Payload);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var payloadObj = JsonSerializer.Deserialize<BusinessTransactionPayload>(json, options);
            
            if (payloadObj != null)
            {
                 if (payloadObj.Currency != "HKD" || (payloadObj.InputType != "cash" && payloadObj.InputType != "front_money"))
                 {
                     if (!services.Contains("service-genesis")) services.Add("service-genesis");
                 }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to parse payload for routing logic, defaulting to CTA only.");
        }

        var saga = new SagaTransaction
        {
            TransactionId = txId,
            Status = "Pending",
            ExpectedServices = services,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        await _repository.CreateAsync(saga);

        var tasks = new List<Task>();
        foreach (var svc in services)
        {
            string appId = svc;
            string method = "api/" + (svc == "service-cta" ? "cta" : "genesis") + "/withdraw";
            tasks.Add(InvokeServiceAsync(appId, method, request));
        }
        
        await Task.WhenAll(tasks);
    }

    public async Task HandleSagaEventAsync(SagaEvent evt)
    {
        Log.Information($"[CashOutSaga] Received event: {evt.ServiceName} - {evt.Status} for {evt.TransactionId}");

        var saga = await _repository.GetByTransactionIdAsync(evt.TransactionId);
        if (saga == null) return;

        if (evt.Status == "Failed")
        {
            if (!saga.FailedServices.Contains(evt.ServiceName)) saga.FailedServices.Add(evt.ServiceName);
        }
        else if (evt.Status == "Success")
        {
            if (!saga.CompletedServices.Contains(evt.ServiceName)) saga.CompletedServices.Add(evt.ServiceName);
        }

        if (saga.FailedServices.Any() && saga.Status != "Compensating" && saga.Status != "Compensated")
        {
             saga.Status = "Compensating";
             await TriggerCompensationAsync(saga);
        }
        else if (saga.CompletedServices.Count >= saga.ExpectedServices.Count && !saga.FailedServices.Any() && saga.Status != "Completed")
        {
            saga.Status = "Completed";
            Log.Information($"[CashOutSaga] Saga {saga.TransactionId} completed successfully.");
        }
        
        saga.UpdateTime = DateTime.UtcNow;
        await _repository.UpdateAsync(x => x.Id == saga.Id, saga);
    }

    private async Task TriggerCompensationAsync(SagaTransaction saga)
    {
        var txRequest = new { TransactionId = saga.TransactionId };
        foreach (var svc in saga.ExpectedServices)
        {
             string method = "api/" + (svc == "service-cta" ? "cta" : "genesis") + "/compensate";
             await InvokeServiceAsync(svc, method, txRequest);
        }
    }

    private async Task<bool> InvokeServiceAsync(string appId, string methodName, object data)
    {
        int maxRetries = _retryOptions.MaxRetries;
        int delayMilliseconds = _retryOptions.InitialDelayMilliseconds;

        for (int i = 0; i < maxRetries; i++)
        {
            // Try Nacos first
            try
            {
                var instance = await _nacosNamingService.SelectOneHealthyInstance(appId, "public");
                if (instance != null)
                {
                    var url = $"http://{instance.Ip}:{instance.Port}/{methodName}";
                    var response = await _httpClient.PostAsJsonAsync(url, data);
                    if (response.IsSuccessStatusCode) return true;
                    Log.Warning($"[CashOutSaga] Nacos invocation failed for {url}: {response.StatusCode}");
                }
                else
                {
                    Log.Warning($"[CashOutSaga] No healthy instance found in Nacos for {appId}");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[CashOutSaga] Error during Nacos invocation for {appId}");
            }

            // Fallback to Dapr
            try
            {
                await _daprClient.InvokeMethodAsync(HttpMethod.Post, appId, methodName, data);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[CashOutSaga] Failed to invoke {appId}/{methodName}. Attempt {i + 1}/{maxRetries}.");
                if (i == maxRetries - 1) return false;
                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2;
            }
        }
        return false;
    }
}
