using System;
using BuildingBlocks.Domain;

namespace OrderService.Domain.ValueObjects;

public sealed record Quantity
{
    private const int MaxValue = 1_000_000;

    public int Value { get; }

    private Quantity(int value) => Value = value;

    public static Quantity From(int value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (value > MaxValue)
            throw new DomainException(
                $"Quantity exceeds maximum allowed value ({MaxValue}).");

        return new Quantity(value);
    }

    public Quantity Add(Quantity other) => From(Value + other.Value);

    public bool IsGreaterThan(Quantity other) => Value > other.Value;

    public override string ToString() => Value.ToString();
}