using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para a entidade <see cref="LicenseDevice"/>.
/// </summary>
public sealed class LicenseDeviceConfiguration : IEntityTypeConfiguration<LicenseDevice>
{
    public void Configure(EntityTypeBuilder<LicenseDevice> builder)
    {
        builder.ToTable("LicenseDevices");

        builder.HasKey(x => x.Id);

        // ========================
        // Properties
        // ========================

        builder.Property(x => x.Id)
               .ValueGeneratedNever();

        builder.Property(x => x.HardwareId)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(x => x.ComputerName)
               .HasMaxLength(256);

        builder.Property(x => x.WindowsVersion)
               .HasMaxLength(128);

        builder.Property(x => x.IpAddress)
               .HasMaxLength(64);

        // ========================
        // Indexes
        // ========================

        // Garante que o mesmo HardwareId não seja registrado mais de uma vez na mesma licença
        builder.HasIndex(x => new { x.LicenseId, x.HardwareId })
               .IsUnique();

        // ========================
        // Relationships
        // ========================

        builder.HasOne(x => x.License)
               .WithMany(x => x.Devices)
               .HasForeignKey(x => x.LicenseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}