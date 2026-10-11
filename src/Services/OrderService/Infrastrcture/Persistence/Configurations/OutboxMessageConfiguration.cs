using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastrcture.Persistence.Outbox;

namespace OrderService.Infrastrcture.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Payload)
            .IsRequired();

        builder.Property(m => m.OccurredAt)
            .IsRequired();

        builder.Property(m => m.ProcessedAt);

        builder.Property(m => m.Error)
            .HasMaxLength(2000);

        builder.HasIndex(m => m.OccurredAt);
    }
}