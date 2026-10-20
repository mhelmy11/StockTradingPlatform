using System;


using ExecutionService.Domain.Executions;
using ExecutionService.Infrastrcture.Messaging;
using ExecutionService.Infrastrcture.Persistence.Inbox;
using ExecutionService.Infrastrcture.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace ExecutionService.Infrastrcture.Persistence;

public sealed class ExecutionDbContext : DbContext
{
    public DbSet<Execution> Executions => Set<Execution>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    public ExecutionDbContext(DbContextOptions<ExecutionDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExecutionDbContext).Assembly);
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
                foreach (var entry in ExecutionEventMapper.Map(domainEvent))
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