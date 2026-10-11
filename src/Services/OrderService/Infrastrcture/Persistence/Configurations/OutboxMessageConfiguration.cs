using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastrcture.Persistence.Outbox;

namespace OrderService.Infrastrcture.Persistence.Configurations;

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

        builder.Property(m => m.OccurredAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(m => m.ProcessedAt)
            .HasColumnType("datetimeoffset");;

        builder.Property(m => m.Error)
            .HasMaxLength(2000);

        builder.HasIndex(m => m.OccurredAt);
    }
}