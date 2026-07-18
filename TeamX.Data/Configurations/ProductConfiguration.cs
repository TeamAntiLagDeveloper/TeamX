using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");


        builder.HasKey(x => x.Id);


        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.Property(x => x.Description)
            .HasMaxLength(500);


        builder.Property(x => x.Price)
            .HasPrecision(18, 2);


        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);



        // Product 1:N License
        builder.HasMany(x => x.Licenses)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}