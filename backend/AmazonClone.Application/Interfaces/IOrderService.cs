using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface IOrderService
{
    Task<Order> CreateAsync(string identityUserId, List<OrderItem> orderItems);
    Task<List<Order>> GetMyOrdersAsync(string identityUserId);
    Task<Order?> GetByIdAsync(Guid orderId, string identityUserId);
    
}