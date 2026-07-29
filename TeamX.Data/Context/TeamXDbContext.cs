using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Data.Configurations;

namespace TeamX.Data.Context;

/// <summary>
/// Contexto principal do Entity Framework Core da aplicação TeamX.
/// </summary>
public sealed class TeamXDbContext : DbContext
{
    public TeamXDbContext(DbContextOptions<TeamXDbContext> options)
        : base(options)
    {
    }

    // ========================
    // DbSets
    // ========================

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<LicenseDevice> LicenseDevices => Set<LicenseDevice>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<UsedNonce> UsedNonces => Set<UsedNonce>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    // ========================
    // Configuration
    // ========================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas as configurações do assembly automaticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeamXDbContext).Assembly);
    }
}