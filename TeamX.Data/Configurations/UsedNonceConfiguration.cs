using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamX.Core.Entities;

namespace TeamX.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para a entidade <see cref="UsedNonce"/>.
/// </summary>
public sealed class UsedNonceConfiguration : IEntityTypeConfiguration<UsedNonce>
{
    public void Configure(EntityTypeBuilder<UsedNonce> builder)
    {
        builder.ToTable("UsedNonces");

        builder.HasKey(x => x.Id);

        // ========================
        // Properties
        // ========================

        builder.Property(x => x.Nonce)
               .HasMaxLength(128)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.ExpiresAt)
               .IsRequired();

        // ========================
        // Indexes
        // ========================

        // Índice único no Nonce (essencial para verificação rápida de replay)
        builder.HasIndex(x => x.Nonce)
               .IsUnique();

        // Índice para limpeza eficiente de nonces expirados
        builder.HasIndex(x => x.ExpiresAt);
    }
}