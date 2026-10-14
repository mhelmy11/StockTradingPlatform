using OrderService.Infrastrcture.Persistence.Outbox;

namespace OrderService.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private const int MaxRetries = 5;

    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public OutboxDestination Destination { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset? NextRetryAt { get; private set; }
    public bool IsDeadLettered { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(
        string type,
        string payload,
        OutboxDestination destination,
        DateTimeOffset occurredAt)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            Destination = destination,
            OccurredAt = occurredAt
        };
    }

    public void MarkAsProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
        LastError = null;
        NextRetryAt = null;
    }

    public void MarkAsFailed(string error)
    {
        RetryCount++;
        LastError = error;

        if (RetryCount >= MaxRetries)
        {
            IsDeadLettered = true;
            NextRetryAt = null;
        }
        else
        {
            // backoff exp
            var delaySeconds = Math.Pow(2, RetryCount);
            NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
        }
    }
}