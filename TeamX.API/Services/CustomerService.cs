using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ApplicationDbContext context,
        ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Customer> GetOrCreateAsync(
        string email,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email é obrigatório.", nameof(email));

        email = email.Trim().ToLowerInvariant();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Email == email, ct);

        if (customer is not null)
            return customer;

        customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = ExtractNameFromEmail(email),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Novo cliente criado: {Email}", email);
            return customer;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogDebug(
                "Cliente {Email} já existia (race condition). Buscando novamente...",
                email);

            _context.Entry(customer).State = EntityState.Detached;

            customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email, ct);

            if (customer is null)
                throw;

            return customer;
        }
    }

    private static string ExtractNameFromEmail(string email)
    {
        var localPart = email.Split('@')[0];
        return localPart.Length > 0 ? localPart : "Cliente";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("IX_", StringComparison.OrdinalIgnoreCase);
    }
}