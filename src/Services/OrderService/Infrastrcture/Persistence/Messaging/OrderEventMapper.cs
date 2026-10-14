using System.Text.Json;
using Contracts.Orders.Commands;
using Contracts.Orders.Events;
using OrderService.Domain.Events;
using OrderService.Infrastrcture.Persistence.Outbox;
using Shared.Domain;

namespace OrderService.Infrastructure.Messaging;

public static class OrderEventMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };


    public static IReadOnlyList<OutboxEntry> Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrderPlacedDomainEvent e => new[]
            {
                Kafka(nameof(OrderPlaced), new OrderPlaced(
                    OrderId: e.OrderId.Value,
                    UserId: e.UserId.Value,
                    Symbol: e.Symbol.Value,
                    Side: (int)e.Side,
                    Type: (int)e.Type,
                    Quantity: e.Quantity.Value,
                    LimitPrice: e.LimitPrice?.Amount,
                    PlacedAt: e.OccurredAt)),

                // Command → RabbitMQ (for Execution)
                RabbitMq(nameof(ExecuteOrder), new ExecuteOrder(
                    OrderId: e.OrderId.Value,
                    UserId: e.UserId.Value,
                    Symbol: e.Symbol.Value,
                    Side: (int)e.Side,
                    Type: (int)e.Type,
                    Quantity: e.Quantity.Value,
                    LimitPrice: e.LimitPrice?.Amount,
                    PlacedAt: e.OccurredAt))
            },

            OrderCancelledDomainEvent e => new[]
            {
                Kafka(nameof(OrderCancelled), new OrderCancelled(
                    OrderId: e.OrderId.Value,
                    UserId: e.UserId.Value,
                    Symbol: e.Symbol.Value,
                    CancelledAt: e.OccurredAt)),

                RabbitMq(nameof(CancelOrderExecution), new CancelOrderExecution(
                    OrderId: e.OrderId.Value,
                    CancelledAt: e.OccurredAt))
            },

            OrderFilledDomainEvent e => new[]
            {
                Kafka(nameof(OrderFilled), new OrderFilled(
                    OrderId: e.OrderId.Value,
                    UserId: e.UserId.Value,
                    Symbol: e.Symbol.Value,
                    FilledQuantity: e.FilledQuantity.Value,
                    ExecutionPrice: e.ExecutionPrice.Amount,
                    NewStatus: (int)e.NewStatus,
                    FilledAt: e.OccurredAt))
            },

            _ => Array.Empty<OutboxEntry>()
        };
    }

    private static OutboxEntry Kafka<T>(string type, T payload) =>
        new(type, JsonSerializer.Serialize(payload, JsonOptions), OutboxDestination.Kafka);

    private static OutboxEntry RabbitMq<T>(string type, T payload) =>
        new(type, JsonSerializer.Serialize(payload, JsonOptions), OutboxDestination.RabbitMQ);

    public readonly record struct OutboxEntry(
        string Type,
        string Payload,
        OutboxDestination Destination);
}