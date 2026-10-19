using System;
using ExecutionService.Infrastrcture.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExecutionService.Infrastrcture.Persistence.Configurations;

public sealed class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("ProcessedMessages");

        // Composite primary key
        builder.HasKey(p => new { p.MessageId, p.ConsumerName });

        builder.Property(p => p.MessageId)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.ConsumerName)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.ProcessedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();
    }
}
