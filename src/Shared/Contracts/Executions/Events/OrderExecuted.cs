using System;

namespace Contracts.Executions.Events;

public sealed record OrderExecuted(
    Guid ExecutionId,
    Guid OrderId,
    Guid UserId,
    string Symbol,
    int Side,
    int Quantity,
    decimal ExecutedPrice,
    DateTimeOffset ExecutedAt);