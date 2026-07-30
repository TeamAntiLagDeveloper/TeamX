using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

public sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(32);

        // Status já é string no domínio (LicenseStatuses.*)
        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.MaxDevices)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.IsActivated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CustomerId);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Licenses)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Licenses)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Licenses)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Devices)
            .WithOne(x => x.License)
            .HasForeignKey(x => x.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}