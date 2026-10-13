using System;
using System.Text.RegularExpressions;
using Shared.Domain;

namespace OrderService.Domain.ValueObjects;

public sealed record Symbol
{
    private static readonly Regex Pattern = new(@"^[A-Z]{1,10}$", RegexOptions.Compiled);

    public string Value { get; }

    private Symbol(string value) => Value = value;

    public static Symbol From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Symbol cannot be empty.");

        var normalized = value.Trim().ToUpperInvariant();

        if (!Pattern.IsMatch(normalized))
            throw new DomainException(
                $"Symbol '{value}' is invalid. Must be 1-10 uppercase letters.");

        return new Symbol(normalized);
    }

    public override string ToString() => Value;
}