using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class OperationCertificateConfiguration : IEntityTypeConfiguration<OperationCertificate>
{
    public void Configure(EntityTypeBuilder<OperationCertificate> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SignatureBase64)
            .IsRequired(); // NVARCHAR(MAX) por defecto en EF Core para string largo

        builder.Property(e => e.IpAddress)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.DeviceFingerprint)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.ServerHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasOne(e => e.RouteStop)
            .WithMany()
            .HasForeignKey(e => e.RouteStopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.RouteStopId);
    }
}
