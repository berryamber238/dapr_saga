namespace Service.PerfectCage.Models;

public class TransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
}

public class TransactionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SagaEvent
{
    public string TransactionId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Success, Failed, Compensated
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
