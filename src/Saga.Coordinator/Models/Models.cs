namespace Saga.Coordinator.Models;

public class SagaInitRequest
{
    public string BusinessId { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
}

public class SagaState
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Compensating, Compensated
    public Dictionary<string, string> ServiceStates { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
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
