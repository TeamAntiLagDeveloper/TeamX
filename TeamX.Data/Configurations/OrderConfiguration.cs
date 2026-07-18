using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);


        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(255)
            .IsRequired();


        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();


        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CustomerId);


        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);


        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId);


        builder.HasOne(x => x.License)
            .WithOne()
            .HasForeignKey<Order>(x => x.LicenseId)
            .IsRequired(false);
    }
}