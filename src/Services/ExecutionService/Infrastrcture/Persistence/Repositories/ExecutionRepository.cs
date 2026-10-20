using System;
using ExecutionService.Domain.Executions;
using Microsoft.EntityFrameworkCore;

namespace ExecutionService.Infrastrcture.Persistence.Repositories;

public sealed class ExecutionRepository : IExecutionRepository
{
    private readonly ExecutionDbContext _db;

    public ExecutionRepository(ExecutionDbContext db) => _db = db;

    public async Task<Execution?> GetByIdAsync(ExecutionId id, CancellationToken ct = default)
        => await _db.Executions.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Execution execution, CancellationToken ct = default)
        => await _db.Executions.AddAsync(execution, ct);
}
