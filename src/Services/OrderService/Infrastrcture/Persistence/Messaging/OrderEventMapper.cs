
using System.Text.Json;
using Shared.Domain;
using Contracts.Orders.Events;
using OrderService.Domain.Events;

namespace OrderService.Infrastrcture.Persistence.Messaging;

public static class OrderEventMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static (string Type, string Payload)? Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrderPlacedDomainEvent e => Serialize(
                nameof(OrderPlaced),
                new OrderPlaced(
                    OrderId: e.OrderId.Value,
                    UserId: e.UserId.Value,
                    Symbol: e.Symbol.Value,
                    Side: (int)e.Side,
                    Type: (int)e.Type,
                    Quantity: e.Quantity.Value,
                    LimitPrice: e.LimitPrice?.Amount,
                    PlacedAt: e.OccurredAt)),

            OrderCancelledDomainEvent e => Serialize(
                nameof(OrderCancelled),
                new OrderCancelled(
                    OrderId: e.OrderId.Value,
                    UserId: e.UserId.Value,
                    Symbol: e.Symbol.Value,
                    CancelledAt: e.OccurredAt)),

            OrderFilledDomainEvent e => Serialize(
                nameof(OrderFilled),
                new OrderFilled(
                    OrderId: e.OrderId.Value,
                    UserId: e.UserId.Value,
                    Symbol: e.Symbol.Value,
                    FilledQuantity: e.FilledQuantity.Value,
                    ExecutionPrice: e.ExecutionPrice.Amount,
                    NewStatus: (int)e.NewStatus,
                    FilledAt: e.OccurredAt)),

            _ => null
        };
    }

    private static (string Type, string Payload) Serialize<T>(string type, T payload)
    {
        return (type, JsonSerializer.Serialize(payload, JsonOptions));
    }
}