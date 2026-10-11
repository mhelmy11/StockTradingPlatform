using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Orders;
using OrderService.Infrastrcture.Persistence.Messaging;
using OrderService.Infrastrcture.Persistence.Outbox;

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
        // 1. Collect domain events from all tracked aggregates
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        // 2. Map each domain event → integration event → outbox row
        var outboxMessages = new List<OutboxMessage>();
        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var mapped = OrderEventMapper.Map(domainEvent);
                if (mapped is null) continue;

                outboxMessages.Add(OutboxMessage.Create(
                    type: mapped.Value.Type,
                    payload: mapped.Value.Payload,
                    occurredAt: domainEvent.OccurredAt));
            }
        }

        // 3. Add outbox rows to the same transaction
        if (outboxMessages.Count > 0)
            OutboxMessages.AddRange(outboxMessages);

        // 4. Save everything in one transaction
        var result = await base.SaveChangesAsync(ct);

        // 5. Clear domain events only after successful save
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        return result;
    }
}