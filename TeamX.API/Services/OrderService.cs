using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;


    public OrderService(
        ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<Order> CreateAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        string email,
        string transactionId)
    {
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


        _context.Orders.Add(order);

        await _context.SaveChangesAsync();


        return order;
    }



    public async Task UpdateLicenseAsync(
        Guid orderId,
        int licenseId)
    {
        var order =
            await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId);


        if (order == null)
            return;


        order.LicenseId = licenseId;


        await _context.SaveChangesAsync();
    }
}