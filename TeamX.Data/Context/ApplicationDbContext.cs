using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;

namespace TeamX.Data.Context;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<LicenseDevice> LicenseDevices => Set<LicenseDevice>();
    public DbSet<LicenseAuditLog> LicenseAuditLogs => Set<LicenseAuditLog>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<UsedNonce> UsedNonces => Set<UsedNonce>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}