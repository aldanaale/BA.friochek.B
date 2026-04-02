using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class RouteStopConfiguration : IEntityTypeConfiguration<RouteStop>
{
    public void Configure(EntityTypeBuilder<RouteStop> builder)
    {
        builder.ToTable("RouteStops");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.HasOne(d => d.Route)
            .WithMany(p => p.Stops)
            .HasForeignKey(d => d.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Order)
            .WithMany()
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Store)
            .WithMany()
            .HasForeignKey(d => d.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
