namespace Saga.Coordinator.Configuration;

public class RetryOptions
{
    public int MaxRetries { get; set; } = 3;
    public int InitialDelayMilliseconds { get; set; } = 1000;
}
