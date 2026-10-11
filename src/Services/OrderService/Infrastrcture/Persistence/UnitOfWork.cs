using System;
using OrderService.Application.Abstraction;

namespace OrderService.Infrastrcture.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _db;

    public UnitOfWork(OrderDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}