using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Orders;
using OrderService.Domain.ValueObjects;

namespace OrderService.Infrastrcture.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.From(value))
            .ValueGeneratedNever();

        builder.Property(o => o.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(o => o.Symbol)
            .HasConversion(
                s => s.Value,
                value => Symbol.From(value))
            .HasMaxLength(10)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(o => o.Side)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.Quantity)
            .HasConversion(
                q => q.Value,
                value => Quantity.From(value))
            .IsRequired();

        builder.Property(o => o.FilledQuantity)
            .HasConversion(
                q => q.Value,
                value => Quantity.From(value))
            .IsRequired();

        builder.Property(o => o.LimitPrice)
            .HasConversion(
                p => p == null ? (decimal?)null : p.Amount,
                value => value == null ? null : Price.From(value.Value))
            .HasColumnType("decimal(18,4)");

        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.PlacedAt)
            .IsRequired()
            .HasColumnType("datetimeoffset");
        ;
        builder.Property(o => o.CancelledAt)
            .HasColumnType("datetimeoffset");
        ;
        builder.Property(o => o.FilledAt)
            .HasColumnType("datetimeoffset");

        // Indexes
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Symbol);
        builder.HasIndex(o => new { o.Status, o.Symbol });

        builder.Ignore(o => o.RemainingQuantity);
        builder.Ignore(o => o.IsOpen);
    }
}