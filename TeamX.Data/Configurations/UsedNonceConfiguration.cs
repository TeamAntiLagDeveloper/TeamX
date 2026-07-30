using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

public sealed class UsedNonceConfiguration : IEntityTypeConfiguration<UsedNonce>
{
    public void Configure(EntityTypeBuilder<UsedNonce> builder)
    {
        builder.ToTable("UsedNonces");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Nonce)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasIndex(x => x.Nonce).IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}