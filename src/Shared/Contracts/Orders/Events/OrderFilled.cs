using System;

namespace Contracts.Orders.Events;

public sealed record OrderFilled(
    Guid OrderId,
    Guid UserId,
    string Symbol,
    int FilledQuantity,
    decimal ExecutionPrice,
    int NewStatus,
    DateTimeOffset FilledAt);