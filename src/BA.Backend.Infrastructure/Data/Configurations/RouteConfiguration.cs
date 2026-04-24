using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(d => d.Transportista)
            .WithMany()
            .HasForeignKey(d => d.TransportistaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Stops)
            .WithOne(e => e.Route)
            .HasForeignKey(e => e.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TenantId, e.Status });
        builder.HasIndex(e => e.TransportistaId);
    }
}
