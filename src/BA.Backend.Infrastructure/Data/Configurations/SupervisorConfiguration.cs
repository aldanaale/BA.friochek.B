using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class SupervisorConfiguration : IEntityTypeConfiguration<Supervisor>
{
    public void Configure(EntityTypeBuilder<Supervisor> builder)
    {
        builder.ToTable("Supervisores");

        builder.HasKey(s => s.UserId);

        builder.Property(s => s.Zone)
            .HasMaxLength(100);

        builder.HasOne(s => s.User)
            .WithOne()
            .HasForeignKey<Supervisor>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
