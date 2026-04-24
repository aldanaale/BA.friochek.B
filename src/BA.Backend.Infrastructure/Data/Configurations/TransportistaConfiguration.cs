using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class TransportistaConfiguration : IEntityTypeConfiguration<Transportista>
{
    public void Configure(EntityTypeBuilder<Transportista> builder)
    {
        builder.ToTable("Transportistas");

        builder.HasKey(t => t.UserId);

        builder.Property(t => t.VehiclePlate)
            .HasMaxLength(20);

        // Sub-tipo de transporte almacenado como TINYINT NULL
        builder.Property(t => t.TransportType).HasConversion<byte?>().IsRequired(false);

        builder.HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<Transportista>(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
