using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface IProductCommentService
{
    Task<List<ProductComment>> GetByProductIdAsync(
        Guid productId, CancellationToken ct = default);
    
    Task<ProductComment?> AddAsync (
        string userId, Guid productId, string text,
        int rating, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(string userId, Guid commentId,
        CancellationToken ct = default);
    
    Task<bool> InternalDeleteAsync (Guid commentId, CancellationToken ct = default);
}