using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Persistence.Outbox;

namespace OrderService.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .HasMaxLength(200)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(m => m.Payload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(m => m.Destination)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.OccurredAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(m => m.ProcessedAt)
            .HasColumnType("datetimeoffset");

        builder.Property(m => m.RetryCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(m => m.LastError)
            .HasMaxLength(2000);

        builder.Property(m => m.NextRetryAt)
            .HasColumnType("datetimeoffset");

        builder.Property(m => m.IsDeadLettered)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(m => new { m.Destination, m.NextRetryAt, m.OccurredAt })
            .HasFilter("[ProcessedAt] IS NULL AND [IsDeadLettered] = 0")
            .HasDatabaseName("IX_OutboxMessages_Pending");
    }
}