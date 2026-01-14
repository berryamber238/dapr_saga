using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DaprDemo.Shared.Models;

public class SagaTransaction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Compensating, Compensated
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
    public object Payload { get; set; } = new();
    public string Status { get; set; } = "Pending"; // Pending, Sent
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime? SentTime { get; set; }
}

public class SharedTransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
}
