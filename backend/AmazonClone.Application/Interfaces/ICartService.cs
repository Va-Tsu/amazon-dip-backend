using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICartService
{
    Task<Cart?> GetMyCartAsync(string identityUserId);
    Task AddItemAsync(string identityUserId, Guid productId, int quantity);
    Task UpdateItemAsync(string identityUserId, Guid cartItemId, int quantity);
    Task RemoveItemAsync(string identityUserId, Guid cartItemId);
    Task ClearAsync(string identityUserId);
    
}