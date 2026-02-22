using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface IOrderService
{
    Task<Order> CreateAsync(string UserId, List<OrderItem> orderItems);
    Task<List<Order>> GetMyOrdersAsync(string UserId);
    Task<Order?> GetByIdAsync(Guid orderId, string UserId);
    
}