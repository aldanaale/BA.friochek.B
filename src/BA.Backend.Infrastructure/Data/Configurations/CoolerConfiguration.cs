using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class CoolerConfiguration : IEntityTypeConfiguration<Cooler>
{
    public void Configure(EntityTypeBuilder<Cooler> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Model).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200).HasDefaultValue("");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);

        // FIX: WithMany(s => s.Coolers) vincula explícitamente la colección
        // en Store, eliminando el shadow property StoreId1
        builder.HasOne(e => e.Store)
            .WithMany(s => s.Coolers)
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.Status });
        builder.HasIndex(e => e.StoreId);
        builder.HasIndex(e => e.SerialNumber).IsUnique();
    }
}
