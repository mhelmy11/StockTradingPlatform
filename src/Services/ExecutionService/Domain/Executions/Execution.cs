using System;
using ExecutionService.Domain.Events;
using Shared.Domain;

namespace ExecutionService.Domain.Executions;

public sealed class Execution : AggregateRoot
{
    public ExecutionId Id { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public string Symbol { get; private set; } = null!;
    public Side Side { get; private set; }
    public int Quantity { get; private set; }
    public decimal ExecutedPrice { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset ExecutedAt { get; private set; }

    private Execution() { }

    public static Execution Complete(
        Guid orderId,
        Guid userId,
        string symbol,
        Side side,
        int quantity,
        decimal executedPrice)
    {
        // invariants
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (executedPrice <= 0)
            throw new DomainException("Price must be positive.");
        if (string.IsNullOrWhiteSpace(symbol))
            throw new DomainException("Symbol required.");

        var execution = new Execution
        {
            Id = ExecutionId.New(),
            OrderId = orderId,
            UserId = userId,
            Symbol = symbol.ToUpperInvariant(),
            Side = side,
            Quantity = quantity,
            ExecutedPrice = executedPrice,
            Status = ExecutionStatus.Completed,
            ExecutedAt = DateTimeOffset.UtcNow
        };

        execution.RaiseDomainEvent(new ExecutionCompletedDomainEvent(
            execution.Id,
            orderId,
            userId,
            symbol,
            side,
            quantity,
            executedPrice,
            execution.ExecutedAt));

        return execution;
    }
}