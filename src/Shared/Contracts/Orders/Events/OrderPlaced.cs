using System;

namespace Contracts.Orders.Events;

public sealed record OrderPlaced(
    Guid OrderId,
    Guid UserId,
    string Symbol,
    int Side,
    int Type,
    int Quantity,
    decimal? LimitPrice,
    DateTimeOffset PlacedAt);
