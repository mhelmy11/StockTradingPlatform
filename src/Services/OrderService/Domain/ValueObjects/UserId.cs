using System;
using BuildingBlocks.Domain;

namespace OrderService.Domain.ValueObjects;

public sealed record UserId
{
    public Guid Value { get; }

    private UserId(Guid value) => Value = value;

    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");
        return new UserId(value);
    }

    public override string ToString() => Value.ToString();
}
