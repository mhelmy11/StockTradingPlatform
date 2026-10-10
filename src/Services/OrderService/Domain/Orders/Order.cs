using System;
using BuildingBlocks.Domain;
using OrderService.Domain.Events;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Orders;

public sealed class Order : AggregateRoot
{
    public OrderId Id { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public Symbol Symbol { get; private set; } = null!;
    public Side Side { get; private set; }
    public OrderType Type { get; private set; }
    public Quantity Quantity { get; private set; } = null!;
    public Quantity FilledQuantity { get; private set; } = null!;
    public Price? LimitPrice { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset? FilledAt { get; private set; }

    private Order() { }

    public static Order Place(
        UserId userId,
        Symbol symbol,
        Side side,
        OrderType type,
        Quantity quantity,
        Price? limitPrice)
    {
        if (type == OrderType.Limit && limitPrice is null)
            throw new DomainException("Limit order requires a limit price.");

        if (type == OrderType.Market && limitPrice is not null)
            throw new DomainException("Market order cannot have a limit price.");

        var order = new Order
        {
            Id = OrderId.New(),
            UserId = userId,
            Symbol = symbol,
            Side = side,
            Type = type,
            Quantity = quantity,
            FilledQuantity = Quantity.From(0),
            LimitPrice = limitPrice,
            Status = OrderStatus.Placed,
            PlacedAt = DateTimeOffset.UtcNow
        };

        order.RaiseDomainEvent(new OrderPlacedDomainEvent(
            order.Id,
            order.UserId,
            order.Symbol,
            order.Side,
            order.Type,
            order.Quantity,
            order.LimitPrice));

        return order;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Filled)
            throw new DomainException("Cannot cancel a fully filled order.");

        if (Status is OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled.");

        if (Status is OrderStatus.Rejected)
            throw new DomainException("Cannot cancel a rejected order.");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id));
    }

    public void ApplyFill(Quantity filledQuantity, Price executionPrice)
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Rejected)
            throw new DomainException($"Cannot fill a {Status} order.");

        if (Status is OrderStatus.Filled)
            throw new DomainException("Order is already fully filled.");

        var newTotal = FilledQuantity.Add(filledQuantity);

        if (newTotal.IsGreaterThan(Quantity))
            throw new DomainException(
                $"Fill of {filledQuantity.Value} would exceed remaining quantity " +
                $"({Quantity.Value - FilledQuantity.Value}).");

        FilledQuantity = newTotal;

        if (FilledQuantity.Value == Quantity.Value)
        {
            Status = OrderStatus.Filled;
            FilledAt = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = OrderStatus.PartiallyFilled;
        }

        RaiseDomainEvent(new OrderFilledDomainEvent(
            Id,
            filledQuantity,
            executionPrice,
            Status));
    }

    public Quantity RemainingQuantity =>
        Quantity.From(Quantity.Value - FilledQuantity.Value);

    public bool IsOpen =>
        Status is OrderStatus.Placed or OrderStatus.PartiallyFilled;
}