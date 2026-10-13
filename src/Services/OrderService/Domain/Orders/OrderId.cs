using System;
using Shared.Domain;

namespace OrderService.Domain.Orders;

public sealed record OrderId
{
    public Guid Value { get; }

    private OrderId(Guid value) => Value = value;

    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("OrderId cannot be empty.");
        return new OrderId(value);
    }

    public override string ToString() => Value.ToString();
}