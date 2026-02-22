using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICartService
{
    Task<Cart?> GetMyCartAsync(string userId, string sessionId, CancellationToken ct = default);
    Task AddItemAsync(string userId, string sessionId, Guid productId, int quantity, CancellationToken ct = default);
    Task UpdateItemAsync(string userId, string sessionId, Guid cartItemId, int quantity, CancellationToken ct = default);
    Task RemoveItemAsync(string userId, string sessionId, Guid cartItemId, CancellationToken ct = default);
    Task ClearAsync(string userId, string sessionId, CancellationToken ct = default);
    Task MergeGuestCartIntoUserAsync(string userId, string cartKey, CancellationToken ct = default);
}
    
