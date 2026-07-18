using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using LicenseEntity = TeamX.Core.Entities.License;

namespace TeamX.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<License> Licenses { get; set; }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Plan> Plans => Set<Plan>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<LicenseDevice> LicenseDevices => Set<LicenseDevice>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}