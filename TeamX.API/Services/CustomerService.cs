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

        // Normaliza: remove espaços e força minúsculo
        email = email.Trim().ToLowerInvariant();

        // 1ª tentativa: busca
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Email == email, ct);

        if (customer is not null)
            return customer;

        // 2ª tentativa: cria
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
            // Race condition: outro request criou o mesmo email quase ao mesmo tempo
            _logger.LogDebug(
                "Cliente {Email} já existia (race condition). Buscando novamente...",
                email);

            // Limpa o change tracker para não ficar com entidade "Added" inválida
            _context.Entry(customer).State = EntityState.Detached;

            customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email, ct);

            if (customer is null)
                throw; // algo muito estranho aconteceu

            return customer;
        }
    }

    private static string ExtractNameFromEmail(string email)
    {
        var localPart = email.Split('@')[0];

        // Remove pontos e números no final se quiser um nome um pouco mais limpo
        // Ex: "joao.silva.123" → "joao.silva"
        return localPart.Length > 0
            ? localPart
            : "Cliente";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // Funciona com SQL Server e PostgreSQL (Npgsql)
        return ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("IX_", StringComparison.OrdinalIgnoreCase) == true;
    }
}