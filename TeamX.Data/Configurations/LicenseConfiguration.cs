    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TeamX.Core.Entities;

    namespace TeamX.Data.Configurations;

    public class LicenseConfiguration : IEntityTypeConfiguration<License>
    {
        public void Configure(
            EntityTypeBuilder<License> builder)
        {
            builder.ToTable("Licenses");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(24);


            builder.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();


            builder.Property(x => x.CreatedAt)
                .IsRequired();


            builder.HasIndex(x => x.Key)
                .IsUnique();



            // Customer 1:N License
            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Licenses)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);



            // Product 1:N License
            builder.HasOne(x => x.Product)
                .WithMany(x => x.Licenses)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);



            // Plan 1:N License
            builder.HasOne(x => x.Plan)
                .WithMany(x => x.Licenses)
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Restrict);



            // License 1:N LicenseDevice
            builder.HasMany(x => x.Devices)
                .WithOne(x => x.License)
                .HasForeignKey(x => x.LicenseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }