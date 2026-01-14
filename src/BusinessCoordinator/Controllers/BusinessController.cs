using DaprSaga.Shared.Models;
using DaprSaga.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BusinessCoordinator.Controllers;

[ApiController]
[Route("api/business")]
public class BusinessController : ControllerBase
{
    private readonly EventStoreRepository _eventStore;

    public BusinessController(EventStoreRepository eventStore)
    {
        _eventStore = eventStore;
    }

    [HttpPost("buy-in")]
    public async Task<IActionResult> BuyIn([FromBody] BuyInTransactionRequest payload)
    {
        return await ProcessRequest(payload, "BuyIn", "BuyIn");
    }

    [HttpPost("cash-out")]
    public async Task<IActionResult> CashOut([FromBody] BusinessTransactionPayload payload)
    {
        return await ProcessRequest(payload, "CashOut", "CashOut");
    }

    private async Task<IActionResult> ProcessRequest(object payload, string eventType, string businessType)
    {
        var txId = Guid.NewGuid().ToString();
        var businessId = Guid.NewGuid().ToString(); 

        var sharedRequest = new SharedTransactionRequest
        {
            TransactionId = txId,
            BusinessId = businessId,
            BusinessType = businessType,
            Payload = payload
        };

        var eventRecord = new EventRecord
        {
            Id = ObjectId.GenerateNewId().ToString(),
            EventType = eventType,
            BusinessType = businessType,
            AggregateId = txId,
            EventData = sharedRequest,
            Timestamp = DateTime.UtcNow
        };

        var outboxMessage = new OutboxMessage
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Topic = string.Empty, // Let Worker route based on BusinessType
            BusinessType = businessType,
            Payload = sharedRequest,
            Status = "Pending",
            CreateTime = DateTime.UtcNow
        };

        await _eventStore.SaveEventWithOutboxAsync(eventRecord, outboxMessage);

        return Ok(new { TransactionId = txId, BusinessId = businessId, Status = "Accepted" });
    }
}
