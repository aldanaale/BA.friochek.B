using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class TechSupportRequestConfiguration : IEntityTypeConfiguration<TechSupportRequest>
{
    public void Configure(EntityTypeBuilder<TechSupportRequest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.FaultType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.PhotoUrls)
            .HasMaxLength(2000);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Technician)
            .WithMany()
            .HasForeignKey(e => e.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Cooler)
            .WithMany()
            .HasForeignKey(e => e.CoolerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(e => new { e.TenantId, e.Status });
        builder.HasIndex(e => e.UserId);
    }
}
