using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICartService
{
    Task<Cart?> GetOrCreateCartAsync(string identityUserId);
    Task AddProductAync(string identityUserId, Guid productId, int quantity);
    
}