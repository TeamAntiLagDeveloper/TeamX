using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext context,
        ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Order> CreateAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        string email,
        string transactionId,
        CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId inválido.", nameof(customerId));

        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId inválido.", nameof(productId));

        if (planId == Guid.Empty)
            throw new ArgumentException("PlanId inválido.", nameof(planId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email é obrigatório.", nameof(email));

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("TransactionId é obrigatório.", nameof(transactionId));

        email = email.Trim().ToLowerInvariant();
        transactionId = transactionId.Trim();

        // Idempotência: webhooks de pagamento podem chegar mais de uma vez
        var existing = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransactionId == transactionId, ct);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Pedido já existia para TransactionId={TransactionId}. OrderId={OrderId}",
                transactionId,
                existing.Id);

            return existing;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            ProductId = productId,
            PlanId = planId,
            CustomerEmail = email,
            TransactionId = transactionId,
            Status = "Paid",
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Pedido criado. OrderId={OrderId} TransactionId={TransactionId} Email={Email}",
                order.Id,
                transactionId,
                email);

            return order;
        }
        catch (DbUpdateException) when (await OrderExistsByTransactionAsync(transactionId, ct))
        {
            // Race condition: outro request criou o mesmo TransactionId
            _context.Entry(order).State = EntityState.Detached;

            var concurrent = await _context.Orders
                .AsNoTracking()
                .FirstAsync(x => x.TransactionId == transactionId, ct);

            _logger.LogInformation(
                "Pedido criado concurrentemente. TransactionId={TransactionId} OrderId={OrderId}",
                transactionId,
                concurrent.Id);

            return concurrent;
        }
    }

    public async Task<bool> UpdateLicenseAsync(
        Guid orderId,
        int licenseId,
        CancellationToken ct = default)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId inválido.", nameof(orderId));

        if (licenseId <= 0)
            throw new ArgumentException("LicenseId inválido.", nameof(licenseId));

        var rows = await _context.Orders
            .Where(x => x.Id == orderId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.LicenseId, licenseId),
                ct);

        if (rows == 0)
        {
            _logger.LogWarning(
                "Tentativa de vincular licença a pedido inexistente. OrderId={OrderId} LicenseId={LicenseId}",
                orderId,
                licenseId);

            return false;
        }

        _logger.LogInformation(
            "Licença vinculada ao pedido. OrderId={OrderId} LicenseId={LicenseId}",
            orderId,
            licenseId);

        return true;
    }

    private Task<bool> OrderExistsByTransactionAsync(string transactionId, CancellationToken ct)
    {
        return _context.Orders
            .AsNoTracking()
            .AnyAsync(x => x.TransactionId == transactionId, ct);
    }
}