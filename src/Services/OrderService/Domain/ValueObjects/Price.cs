using System;
using Shared.Domain;

namespace OrderService.Domain.ValueObjects;

public sealed record Price
{
    public const int Scale = 4;
    private const decimal MaxAmount = 1_000_000m;

    public decimal Amount { get; }

    private Price(decimal amount) => Amount = amount;

    public static Price From(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Price must be greater than zero.");

        if (amount > MaxAmount)
            throw new DomainException(
                $"Price exceeds maximum allowed value ({MaxAmount}).");

        return new Price(Math.Round(amount, Scale, MidpointRounding.ToEven));
    }

    public override string ToString() => Amount.ToString($"F{Scale}");
}