using System;

namespace Contracts.Orders.Commands;

public sealed record ExecuteOrder(
    Guid OrderId,
    Guid UserId,
    string Symbol,
    int Side,
    int Type,
    int Quantity,
    decimal? LimitPrice,
    DateTimeOffset PlacedAt);