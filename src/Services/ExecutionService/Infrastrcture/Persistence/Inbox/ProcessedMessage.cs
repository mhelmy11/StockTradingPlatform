using System;

namespace ExecutionService.Infrastrcture.Persistence.Inbox;

public sealed class ProcessedMessage
{
    public string MessageId { get; private set; } = null!;
    public string ConsumerName { get; private set; } = null!;
    public DateTimeOffset ProcessedAt { get; private set; }

    private ProcessedMessage() { }

    public static ProcessedMessage Create(string messageId, string consumerName)
    {
        return new ProcessedMessage
        {
            MessageId = messageId,
            ConsumerName = consumerName,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }
}
