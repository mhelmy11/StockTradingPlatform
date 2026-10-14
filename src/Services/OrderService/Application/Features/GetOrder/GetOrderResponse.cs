using System;

namespace OrderService.Application.Features.GetOrder;

public sealed record GetOrderResponse(
    Guid OrderId,
    Guid UserId,
    string Symbol,
    int Side,
    int Type,
    int Quantity,
    int FilledQuantity,
    int RemainingQuantity,
    decimal? LimitPrice,
    int Status,
    string StatusText,
    DateTimeOffset PlacedAt,
    DateTimeOffset? FilledAt,
    DateTimeOffset? CancelledAt);
