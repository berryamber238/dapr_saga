using Dapr;
using Microsoft.AspNetCore.Mvc;
using Saga.Coordinator.Services;

namespace Saga.Coordinator.Controllers;

[ApiController]
[Route("api/saga")]
public class SagaController : ControllerBase
{
    private readonly ISagaOrchestrator _orchestrator;

    public SagaController(ISagaOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
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
}
