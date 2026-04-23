using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status).IsRequired().HasMaxLength(50);
        builder.Property(o => o.NfcTagId).HasMaxLength(100);
        builder.Property(o => o.ExternalOrderId).HasMaxLength(200);
        builder.Property(o => o.ExternalStatus).HasMaxLength(100);

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Cooler)
            .WithMany()
            .HasForeignKey(o => o.CoolerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tell EF Core to use the private _items backing field so IReadOnlyCollection<OrderItem>
        // on the aggregate root does not need a public setter.
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.TenantId, o.Status });
        builder.HasIndex(o => o.UserId);
    }
}
