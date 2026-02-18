using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Domain.Enums;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class OrderService : IOrderService
{
    readonly ApplicationDbContext _dbContext;

    public OrderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Order> CreateAsync(string identityUserId, List<OrderItem> orderItems)
    {
        var productIds = orderItems.Select(i => i.ProductId).ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            IdentityUserId = identityUserId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };
        
        decimal total = 0;

        foreach (var item in orderItems)
        {
            var product = products.First(p => p.Id == item.ProductId);
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = item.Quantity
            });
            
            total += product.Price * item.Quantity;
        }
        
        order.TotalPrice = total;
        
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();
        return order;

    }

    public async Task<List<Order>> GetMyOrdersAsync(string identityUserId)
    {
        return await _dbContext.Orders.
            Where(o=>o.IdentityUserId == identityUserId)
            .Include(o=>o.Items)
            .OrderByDescending(o=>o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, string identityUserId)
    {
        return await _dbContext.Orders
            .Include(o=>o.Items)
            .FirstOrDefaultAsync(o=>o.Id == orderId && o.IdentityUserId == identityUserId);
    }
}