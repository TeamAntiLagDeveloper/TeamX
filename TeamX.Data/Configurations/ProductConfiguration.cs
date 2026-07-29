using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para a entidade <see cref="Product"/>.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        // ========================
        // Properties
        // ========================

        builder.Property(x => x.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Code)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        builder.Property(x => x.Type)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(x => x.Price)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(x => x.IsActive)
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        // ========================
        // Indexes
        // ========================

        builder.HasIndex(x => x.Code)
               .IsUnique();

        builder.HasIndex(x => x.Name);

        // ========================
        // Relationships
        // ========================

        // Product 1:N License
        builder.HasMany(x => x.Licenses)
               .WithOne(x => x.Product)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // Product 1:N Plan
        builder.HasMany(x => x.Plans)
               .WithOne(x => x.Product)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}