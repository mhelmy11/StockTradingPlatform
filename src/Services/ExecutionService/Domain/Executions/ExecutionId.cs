using System;
using Shared.Domain;

namespace ExecutionService.Domain.Executions;

public sealed record ExecutionId
{
    public Guid Value { get; }
    private ExecutionId(Guid value) => Value = value;
    public static ExecutionId New() => new(Guid.NewGuid());
    public static ExecutionId From(Guid value)
    {
        if (value == Guid.Empty || Guid.TryParse(value.ToString(), out _) == false)
            throw new DomainException("ExecutionId cannot be empty.");
        return new ExecutionId(value);
    }
}