using System;
using ExecutionService.Domain.Executions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExecutionService.Infrastrcture.Persistence.Configurations;


public sealed class ExecutionConfiguration : IEntityTypeConfiguration<Execution>
{
    public void Configure(EntityTypeBuilder<Execution> builder)
    {
        builder.ToTable("Executions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => ExecutionId.From(value))
            .ValueGeneratedNever();

        builder.Property(e => e.OrderId).IsRequired();
        builder.Property(e => e.UserId).IsRequired();

        builder.Property(e => e.Symbol)
            .HasMaxLength(10)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(e => e.Side)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Quantity).IsRequired();

        builder.Property(e => e.ExecutedPrice)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ExecutedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.Symbol, e.ExecutedAt });
    }
}