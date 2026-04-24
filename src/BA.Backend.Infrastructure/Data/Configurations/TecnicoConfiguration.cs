using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class TecnicoConfiguration : IEntityTypeConfiguration<Tecnico>
{
    public void Configure(EntityTypeBuilder<Tecnico> builder)
    {
        builder.ToTable("Tecnicos");

        builder.HasKey(t => t.UserId);

        builder.Property(t => t.Specialty)
            .HasMaxLength(100);

        builder.HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<Tecnico>(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
