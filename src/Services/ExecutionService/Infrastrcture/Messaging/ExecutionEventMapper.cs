using System;

using System.Text.Json;
using Contracts.Executions.Events;
using ExecutionService.Domain.Events;
using ExecutionService.Infrastrcture.Persistence.Outbox;
using Shared.Domain;


namespace ExecutionService.Infrastrcture.Messaging;

public static class ExecutionEventMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static IReadOnlyList<OutboxEntry> Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ExecutionCompletedDomainEvent e => new[]
            {
                // Event → Kafka (Debezium) —  Portfolio + Notifications + Analytics
                Kafka(nameof(OrderExecuted), new OrderExecuted(
                    ExecutionId: e.ExecutionId.Value,
                    OrderId: e.OrderId,
                    UserId: e.UserId,
                    Symbol: e.Symbol,
                    Side: (int)e.Side,
                    Quantity: e.Quantity,
                    ExecutedPrice: e.ExecutedPrice,
                    ExecutedAt: e.ExecutedAt))
            },
            _ => Array.Empty<OutboxEntry>()
        };
    }

    private static OutboxEntry Kafka<T>(string type, T payload) =>
        new(type, JsonSerializer.Serialize(payload, JsonOptions), OutboxDestination.Kafka);

    public readonly record struct OutboxEntry(string Type, string Payload, OutboxDestination Destination);
}
