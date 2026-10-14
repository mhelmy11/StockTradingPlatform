using System.Text.Json;
using Contracts.Orders.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Infrastrcture.Persistence;
using OrderService.Infrastrcture.Persistence.Outbox;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Outbox;

namespace OrderService.Infrastructure.Messaging;

public sealed class RabbitMqOutboxPublisher : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan FailureDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBus _bus;
    private readonly ILogger<RabbitMqOutboxPublisher> _logger;

    public RabbitMqOutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IBus bus,
        ILogger<RabbitMqOutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("RabbitMQ Outbox Publisher started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessBatchAsync(ct);

                if (processed == 0)
                    await Task.Delay(IdleDelay, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Outbox publisher loop failed. Retrying in {Delay}s.",
                    FailureDelay.TotalSeconds);
                await Task.Delay(FailureDelay, ct);
            }
        }

        _logger.LogInformation("RabbitMQ Outbox Publisher stopped.");
    }

    private async Task<int> ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var now = DateTimeOffset.UtcNow;

        var messages = await db.OutboxMessages
            .Where(m => m.Destination == OutboxDestination.RabbitMQ
                     && m.ProcessedAt == null
                     && !m.IsDeadLettered
                     && (m.NextRetryAt == null || m.NextRetryAt <= now))
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return 0;

        foreach (var msg in messages)
        {
            try
            {
                await PublishAsync(msg, ct);
                msg.MarkAsProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish outbox message {MessageId} of type {Type}. Attempt {Attempt}.",
                    msg.Id, msg.Type, msg.RetryCount + 1);

                msg.MarkAsFailed(ex.Message);

                if (msg.IsDeadLettered)
                {
                    _logger.LogError(
                        "Outbox message {MessageId} dead-lettered after {Retries} attempts. Last error: {Error}",
                        msg.Id, msg.RetryCount, ex.Message);
                }
            }

            await db.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Processed {Count} outbox messages.", messages.Count);
        return messages.Count;
    }

    private async Task PublishAsync(OutboxMessage msg, CancellationToken ct)
    {
        object? command = msg.Type switch
        {
            nameof(ExecuteOrder) =>
                JsonSerializer.Deserialize<ExecuteOrder>(msg.Payload),
            nameof(CancelOrderExecution) =>
                JsonSerializer.Deserialize<CancelOrderExecution>(msg.Payload),
            _ => null
        };

        if (command is null)
        {
            _logger.LogWarning("Unknown outbox message type: {Type}", msg.Type);
            throw new InvalidOperationException($"Unknown outbox type: {msg.Type}");
        }

        await _bus.Publish(command, context =>
        {
            context.MessageId = msg.Id;
        }, ct);
    }
}