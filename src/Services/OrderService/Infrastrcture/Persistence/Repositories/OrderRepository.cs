

using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Orders;


namespace OrderService.Infrastrcture.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _db;

    public OrderRepository(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<Order?> GetByIdAsync(
        OrderId id,
        CancellationToken ct = default)
    {
        return await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(
        Order order,
        CancellationToken ct = default)
    {
        await _db.Orders.AddAsync(order, ct);
    }
}
