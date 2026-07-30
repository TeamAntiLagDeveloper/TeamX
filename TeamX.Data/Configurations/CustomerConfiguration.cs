using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FullName).HasMaxLength(200);
        builder.Property(x => x.Discord).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(80);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
    }
}