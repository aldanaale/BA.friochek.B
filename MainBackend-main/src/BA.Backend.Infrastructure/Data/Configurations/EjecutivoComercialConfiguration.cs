using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class EjecutivoComercialConfiguration : IEntityTypeConfiguration<EjecutivoComercial>
{
    public void Configure(EntityTypeBuilder<EjecutivoComercial> builder)
    {
        builder.ToTable("EjecutivosComerciales");

        builder.HasKey(e => e.UserId);

        builder.Property(e => e.Territory)
            .HasMaxLength(100);

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<EjecutivoComercial>(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
