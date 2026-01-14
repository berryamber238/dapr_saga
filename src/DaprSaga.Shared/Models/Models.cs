using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DaprSaga.Shared.Models;

public class SagaTransaction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Compensating, Compensated
    public List<string> ExpectedServices { get; set; } = new();
    public List<string> FailedServices { get; set; } = new();
    public List<string> CompletedServices { get; set; } = new();
    public string CompensateStatus { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}

public class BusinessData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string BusinessId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}

public class CompensateLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string LogId { get; set; } = Guid.NewGuid().ToString();
    public string TransactionId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime CompensateTime { get; set; } = DateTime.UtcNow;
    public string Result { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class EventRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string EventType { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public object EventData { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class OutboxMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string Topic { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public string Status { get; set; } = "Pending"; // Pending, Sent
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime? SentTime { get; set; }
}

public class SharedTransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty; // Added
    public object Payload { get; set; } = new();
}

public class BusinessTransactionPayload
{
    public string InputType { get; set; } = string.Empty; // cash, credit_card, etc.
    public string Currency { get; set; } = "HKD";
    public decimal Amount { get; set; }
    // Additional fields can be mapped from dynamic payload
}

public class BuyInInputPayload
{
    public string[] InputType { get; set; } = Array.Empty<string>();
    public decimal InputAmount { get; set; }
}

public class BuyInOutputPayload
{
    public string[] OutputType { get; set; } = Array.Empty<string>(); // CC, NN, LPG
    public decimal OutputAmount { get; set; }
}

public class BuyInTransactionRequest
{
    public BuyInInputPayload Input { get; set; } = new();
    public BuyInOutputPayload Output { get; set; } = new();
    public string Currency { get; set; } = "HKD";
}

public class TransactionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object Data { get; set; } = new();
}
