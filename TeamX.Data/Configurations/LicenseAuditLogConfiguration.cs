using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

public sealed class LicenseAuditLogConfiguration : IEntityTypeConfiguration<LicenseAuditLog>
{
    public void Configure(EntityTypeBuilder<LicenseAuditLog> builder)
    {
        builder.ToTable("LicenseAuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.EventType).IsRequired().HasMaxLength(40);
        builder.Property(x => x.HardwareId).HasMaxLength(128);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.Details).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.LicenseId, x.CreatedAt });
        builder.HasIndex(x => x.EventType);

        builder.HasOne(x => x.License)
            .WithMany()
            .HasForeignKey(x => x.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}