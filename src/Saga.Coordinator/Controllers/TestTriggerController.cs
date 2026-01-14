using DaprSaga.Shared.Models;
using DaprSaga.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;
using Saga.Coordinator.Models;
using MongoDB.Bson;

namespace Saga.Coordinator.Controllers;

[ApiController]
[Route("api/test")]
public class TestTriggerController : ControllerBase
{
    private readonly EventStoreRepository _eventStore;

    public TestTriggerController(EventStoreRepository eventStore)
    {
        _eventStore = eventStore;
    }

    [HttpPost("trigger-via-eventstore")]
    public async Task<IActionResult> TriggerSaga([FromBody] TransactionRequest request)
    {
        // 1. Generate TransactionId if not provided
        var txId = string.IsNullOrEmpty(request.TransactionId) ? Guid.NewGuid().ToString() : request.TransactionId;
        request.TransactionId = txId;

        // Ensure Payload is a simple object, not JsonElement
        var payloadObj = request.Payload;
        if (payloadObj is System.Text.Json.JsonElement jsonElement)
        {
            // Convert JsonElement back to a dictionary/dynamic object that MongoDB driver can handle
            var json = jsonElement.GetRawText();
            payloadObj = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<System.Dynamic.ExpandoObject>(json);
            request.Payload = payloadObj;
        }

        // Use Shared Model to avoid Discriminator issues across services
        var sharedRequest = new SharedTransactionRequest
        {
            TransactionId = request.TransactionId,
            BusinessId = request.BusinessId,
            Payload = request.Payload
        };

        // 2. Create EventRecord (simulating "BuyInStarted")
        var eventRecord = new EventRecord
        {
            Id = ObjectId.GenerateNewId().ToString(),
            EventType = "SagaInit",
            AggregateId = txId,
            EventData = sharedRequest,
            Timestamp = DateTime.UtcNow
        };

        // 3. Create Outbox Message (to be picked up by Worker)
        var outboxMessage = new OutboxMessage
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Topic = "saga-init", // Topic name
            Payload = sharedRequest,
            Status = "Pending",
            CreateTime = DateTime.UtcNow
        };

        // 4. Atomic Write
        await _eventStore.SaveEventWithOutboxAsync(eventRecord, outboxMessage);

        return Ok(new { TransactionId = txId, Status = "EventQueued" });
    }
}
