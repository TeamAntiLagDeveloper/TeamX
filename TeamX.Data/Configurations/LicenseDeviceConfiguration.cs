using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

public class LicenseDeviceConfiguration : IEntityTypeConfiguration<LicenseDevice>
{
    public void Configure(EntityTypeBuilder<LicenseDevice> builder)
    {
        builder.ToTable("LicenseDevices");

        builder.HasKey(x => x.Id);

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

        builder.HasOne(x => x.License)
               .WithMany(x => x.Devices)
               .HasForeignKey(x => x.LicenseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}