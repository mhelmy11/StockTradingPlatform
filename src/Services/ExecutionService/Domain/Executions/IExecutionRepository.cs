using System;

namespace ExecutionService.Domain.Executions;

public interface IExecutionRepository
{
    Task<Execution?> GetByIdAsync(ExecutionId id, CancellationToken ct = default);
    Task AddAsync(Execution execution, CancellationToken ct = default);
}
