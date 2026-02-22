using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICartService
{
    Task<Cart?> GetMyCartAsync(string UserId);
    Task AddItemAsync(string UserId, Guid productId, int quantity);
    Task UpdateItemAsync(string UserId, Guid cartItemId, int quantity);
    Task RemoveItemAsync(string UserId, Guid cartItemId);
    Task ClearAsync(string UserId);
    
}