using System;
using ExecutionService.Domain.Executions;
using Shared.Domain;

namespace ExecutionService.Domain.Events;

public sealed record ExecutionCompletedDomainEvent(
    ExecutionId ExecutionId,
    Guid OrderId,
    Guid UserId,
    string Symbol,
    Side Side,
    int Quantity,
    decimal ExecutedPrice,
    DateTimeOffset ExecutedAt)
    : DomainEvent;