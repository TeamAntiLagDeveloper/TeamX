using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TeamX.Data.Context;

namespace TeamX.Data.Factories;

public class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=db.zttfeyhevvithrhppiye.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TeamXProject@w;SSL Mode=Require;Trust Server Certificate=true"
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}