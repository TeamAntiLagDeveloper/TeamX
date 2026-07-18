using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;


    public CustomerService(
        ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<Customer> GetOrCreateAsync(
        string email)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.Email == email);


        if (customer != null)
            return customer;


        customer = new Customer
        {
            Id = Guid.NewGuid(),

            Email = email,

            FullName = email.Split('@')[0],

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };


        _context.Customers.Add(customer);


        await _context.SaveChangesAsync();


        return customer;
    }
}