using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class ProductCommentService :IProductCommentService
{
    readonly ApplicationDbContext _dbContext;

    public ProductCommentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<ProductComment>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await _dbContext.ProductComments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.ProductId == productId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    }

    public async Task<ProductComment?> AddAsync(string userId, Guid productId, string text, int rating, CancellationToken ct = default)
    {
        var productExists = await _dbContext.Products
            .AnyAsync(p => p.Id == productId && p.IsActive, ct);

        if (!productExists)
        {
            return null;
        }

        if (string.IsNullOrEmpty(text))
        {
            throw new Exception("Comment text is required");
        }

        if (rating < 1 || rating > 5)
        {
            throw new Exception("Rating must be between 1 and 5");
        }

        var comment = new ProductComment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            Text = text.Trim(),
            Rating = rating,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _dbContext.ProductComments.Add(comment);
        await _dbContext.SaveChangesAsync(ct);
        return await _dbContext.ProductComments
            .AsNoTracking()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id, ct);
    }

    public async Task<bool> DeleteAsync(string userId, Guid commentId, CancellationToken ct = default)
    {
        var comment = await _dbContext.ProductComments
            .FirstOrDefaultAsync(c=>c.Id == commentId && !c.IsDeleted, ct);
        if (comment == null)
        {
            return false;
        }

        if (comment.UserId != userId)
        {
            return false;
        }
        
        comment.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> InternalDeleteAsync(Guid commentId, CancellationToken ct = default)
    {
        var comment = await _dbContext.ProductComments
            .FirstOrDefaultAsync(c=>c.Id == commentId && !c.IsDeleted, ct);
        if (comment == null)
        {
            return false;
        }
        
        comment.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}