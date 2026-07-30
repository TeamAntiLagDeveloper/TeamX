using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Enums;
using TeamX.Data.Context;

namespace TeamX.Data.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken))
            return;

        var productId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var product = new Product
        {
            Id = productId,
            Name = "TeamX Optimizer",
            Code = "TEAMX",
            Description = "Otimizador TeamX",
            Type = ProductType.TeamX,
            Price = 0m,
            IsActive = true,
            CreatedAt = now
        };

        // Codes = variant IDs do Eremby (ajuste se o painel mudar)
        var plans = new[]
        {
            new Plan
            {
                PlanId = Guid.NewGuid(),
                ProductId = productId,
                Name = "Basic",
                Code = "112169",
                DurationDays = 30,
                Price = 29.90m,
                MaxDevices = 1,
                IsLifetime = false,
                IsActive = true,
                CreatedAt = now
            },
            new Plan
            {
                PlanId = Guid.NewGuid(),
                ProductId = productId,
                Name = "Standard",
                Code = "112170",
                DurationDays = 90,
                Price = 59.90m,
                MaxDevices = 2,
                IsLifetime = false,
                IsActive = true,
                CreatedAt = now
            },
            new Plan
            {
                PlanId = Guid.NewGuid(),
                ProductId = productId,
                Name = "Professional",
                Code = "112171",
                DurationDays = 365,
                Price = 149.90m,
                MaxDevices = 3,
                IsLifetime = false,
                IsActive = true,
                CreatedAt = now
            }
        };

        db.Products.Add(product);
        db.Plans.AddRange(plans);
        await db.SaveChangesAsync(cancellationToken);
    }
}