using Shared.Domain;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Orders;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence.Outbox;

namespace OrderService.Infrastrcture.Persistence;


public sealed class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var outboxMessages = new List<OutboxMessage>();
        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                foreach (var entry in OrderEventMapper.Map(domainEvent))
                {
                    outboxMessages.Add(OutboxMessage.Create(
                        type: entry.Type,
                        payload: entry.Payload,
                        destination: entry.Destination,
                        occurredAt: domainEvent.OccurredAt));
                }
            }
        }

        if (outboxMessages.Count > 0)
            OutboxMessages.AddRange(outboxMessages);

        var result = await base.SaveChangesAsync(ct);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        return result;
    }
}