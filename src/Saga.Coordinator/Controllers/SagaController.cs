using Dapr;
using Microsoft.AspNetCore.Mvc;
using Saga.Coordinator.Services;
using DaprSaga.Shared.Models;

namespace Saga.Coordinator.Controllers;

[ApiController]
[Route("api/saga")]
public class SagaController : ControllerBase
{
    private readonly ISagaOrchestrator _orchestrator;
    private readonly BuyInSagaOrchestrator _buyInOrchestrator;
    private readonly CashOutSagaOrchestrator _cashOutOrchestrator;

    public SagaController(ISagaOrchestrator orchestrator, BuyInSagaOrchestrator buyIn, CashOutSagaOrchestrator cashOut)
    {
        _orchestrator = orchestrator;
        _buyInOrchestrator = buyIn;
        _cashOutOrchestrator = cashOut;
    }

    [HttpPost("init")]
    public async Task<IActionResult> InitSaga([FromBody] TransactionRequest request)
    {
        var txId = await _orchestrator.InitSagaAsync(request);
        return Ok(new { TransactionId = txId, Status = "Pending" });
    }

    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetStatus(string transactionId)
    {
        var state = await _orchestrator.GetSagaStateAsync(transactionId);
        if (state == null) return NotFound();
        return Ok(state);
    }

    [Topic("pubsub", "saga-status")]
    [HttpPost("status-handler")]
    public async Task<IActionResult> HandleStatusEvent([FromBody] SagaEvent evt)
    {
        await _orchestrator.HandleSagaEventAsync(evt);
        return Ok();
    }

    [Topic("pubsub", "saga-init")]
    [HttpPost("init-handler")]
    public async Task<IActionResult> HandleInitEvent([FromBody] TransactionRequest request)
    {
        Console.WriteLine($"[Coordinator] Received Saga Init Event for {request.TransactionId}");
        
        // Check if Saga already exists to avoid duplicate processing
        var existing = await _orchestrator.GetSagaStateAsync(request.TransactionId);
        if (existing != null)
        {
             Console.WriteLine($"[Coordinator] Saga {request.TransactionId} already exists, skipping init.");
             return Ok();
        }

        await _orchestrator.InitSagaAsync(request);
        return Ok();
    }

    [Topic("pubsub", "saga-buyin")]
    [HttpPost("buyin-handler")]
    public async Task<IActionResult> HandleBuyInEvent([FromBody] SharedTransactionRequest request)
    {
        Console.WriteLine($"[Coordinator] Received Saga BuyIn Event for {request.TransactionId}");
        var existing = await _orchestrator.GetSagaStateAsync(request.TransactionId);
        if (existing != null) return Ok();

        await _buyInOrchestrator.InitSagaAsync(request);
        return Ok();
    }

    [Topic("pubsub", "saga-cashout")]
    [HttpPost("cashout-handler")]
    public async Task<IActionResult> HandleCashOutEvent([FromBody] SharedTransactionRequest request)
    {
        Console.WriteLine($"[Coordinator] Received Saga CashOut Event for {request.TransactionId}");
        var existing = await _orchestrator.GetSagaStateAsync(request.TransactionId);
        if (existing != null) return Ok();

        await _cashOutOrchestrator.InitSagaAsync(request);
        return Ok();
    }
}
