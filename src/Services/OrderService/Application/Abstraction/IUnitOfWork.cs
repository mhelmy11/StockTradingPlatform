using System;

namespace OrderService.Application.Abstraction;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}