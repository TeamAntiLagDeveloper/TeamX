using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para a entidade <see cref="Order"/>.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        // ========================
        // Properties
        // ========================

        builder.Property(x => x.CustomerEmail)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();

        // ========================
        // Indexes
        // ========================

        builder.HasIndex(x => x.CustomerEmail);
        builder.HasIndex(x => x.Status);

        // ========================
        // Relationships
        // ========================

        // Customer 1:N Order
        builder.HasOne(x => x.Customer)
               .WithMany(x => x.Orders)
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        // Product 1:N Order
        builder.HasOne(x => x.Product)
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // Plan 1:N Order
        builder.HasOne(x => x.Plan)
               .WithMany()
               .HasForeignKey(x => x.PlanId)
               .OnDelete(DeleteBehavior.Restrict);

        // Order 1:1 License (opcional)
        builder.HasOne(x => x.License)
               .WithOne()
               .HasForeignKey<Order>(x => x.LicenseId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}