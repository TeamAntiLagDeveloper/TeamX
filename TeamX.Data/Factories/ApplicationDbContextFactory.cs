using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TeamX.Data.Context;

namespace TeamX.Data.Factories;

/// <summary>
/// Factory usada pelo Entity Framework Core em tempo de design
/// (migrations, scaffolding, etc.).
/// </summary>
public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("TEAMX_DB_CONNECTION")
            ?? throw new InvalidOperationException(
                "A variável de ambiente 'TEAMX_DB_CONNECTION' não está configurada.");

        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}