using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICartService
{
    Task<Cart?> GetCartAsync(string userId, string cartKey, CancellationToken ct = default);
    Task AddItemAsync(string userId, string cartKey, Guid productId, int quantity, CancellationToken ct = default);
    Task UpdateItemAsync(string userId, string cartKey, Guid cartItemId, int quantity, CancellationToken ct = default);
    Task RemoveItemAsync(string userId, string cartKey, Guid cartItemId, CancellationToken ct = default);
    Task ClearAsync(string userId, string cartKey, CancellationToken ct = default);
    Task MergeGuestCartIntoUserAsync(string userId, string cartKey, CancellationToken ct = default);
    Task<Guid> CheckoutAsync(string? userId, string cartKey, CancellationToken ct = default);
}
    
