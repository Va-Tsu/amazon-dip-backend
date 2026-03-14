using AmazonClone.Application.Helpers;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class ProductService : IProductService
{
    readonly ApplicationDbContext _dbContext;

    public ProductService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<Product>> GetAllAsync()
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Discounts)
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.Id == id && p.IsActive)
            .FirstOrDefaultAsync(ct);

        if (product == null)
        {
            return null;
        }
        DiscountHelper.ApplyDiscount(product);
        return product;
    }
    
    public async Task<List<Product>> SearchAsync(string query)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.Name.Contains(query) && p.IsActive)
            .ToListAsync();
    }

    public async Task<List<Product>> GetNewAsync()
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.CreatedAt > DateTime.UtcNow.AddDays(-14))
            .ToListAsync();
    }

    public async Task<(List<Product>, bool hasMore)> GetRecommendedAsync(string? userId,
        int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize <= 0 ? 8 : pageSize;
        
        var skip = (page - 1) * pageSize;
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive);
        if (string.IsNullOrEmpty(userId))
        {
            query = query.OrderByDescending(p => p.CreatedAt);
            var products = await query.Skip(skip)
                .Take(pageSize + 1)
                .ToListAsync(ct);
            
            var hasMore = products.Count>pageSize;

            if (hasMore)
            {
                products = products.Take(pageSize).ToList();
            }
            
            return (products, hasMore);
        }

        var oldViewed = await _dbContext.RecentlyViewedProducts
            .Where(p => p.UserId == userId)
            .OrderByDescending(r => r.ViewedAt)
            .Skip(50)
            .ToListAsync(ct);
        
        if (oldViewed.Any())
        {
            _dbContext.RecentlyViewedProducts.RemoveRange(oldViewed);
            await _dbContext.SaveChangesAsync(ct);
        }
        
        var recentViewedIds = await _dbContext.RecentlyViewedProducts
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(r => r.ViewedAt)
            .Take(10)
            .Select(r => r.ProductId)
            .ToListAsync(ct);

        if (!recentViewedIds.Any())
        {
            query = query.OrderByDescending(p => p.CreatedAt);
            var products = await query.Skip(skip)
                .Take(pageSize + 1)
                .ToListAsync(ct);
            
            var hasMore = products.Count>pageSize;

            if (hasMore)
            {
                products = products.Take(pageSize).ToList();
            }
            return (products, hasMore);
        }

        var viewedMeta = await _dbContext.Products
            .AsNoTracking().Where(p => recentViewedIds.Contains(p.Id))
            .Select(p => new
            {
                p.CategoryId,
                p.CountryId
            })
            .ToListAsync(ct);
        
        var categoryIds = viewedMeta
            .Select(p => p.CategoryId)
            .Distinct()
            .ToList();
        var countryIds = viewedMeta
            .Select(p => p.CountryId)
            .Distinct()
            .ToList();
        
        query = query.Where(p => !recentViewedIds.Contains(p.Id))
            .OrderByDescending(p=>
                (categoryIds.Contains(p.CategoryId)? 2 : 0)+
                (countryIds.Contains(p.CountryId) ? 1 : 0))
            .ThenByDescending(p=>p.CreatedAt);
        var result =  await query
            .Skip(skip)
            .Take(pageSize+1)
            .ToListAsync(ct);
        var hasMoreResult = result.Count>pageSize;
        if (hasMoreResult)
        {
            result = result.Take(pageSize).ToList();
        }
        
        return(result, hasMoreResult);
    }

    public async Task AddRecentlyViewedAsync(string userId, Guid productId, CancellationToken ct = default)
    {
        var existing = await _dbContext.RecentlyViewedProducts
            .FirstOrDefaultAsync(r=>r.UserId == userId && 
                                    r.ProductId == productId, ct);
        if (existing != null)
        {
            existing.ViewedAt = DateTime.UtcNow;
        }
        else
        {
            _dbContext.RecentlyViewedProducts.Add(
                new RecentlyViewedProduct
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId,
                    ViewedAt = DateTime.UtcNow
                });
        }
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Guid> CreateAsync(Product product, CancellationToken ct = default)
    {
        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = product.Name,
            Description = product.Description,
            Weight = product.Weight,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CountryId = product.CountryId,
            CreatedAt = DateTime.Now,
            ImageUrl = product.ImageUrl
        };
        _dbContext.Products.Add(newProduct);
        await _dbContext.SaveChangesAsync(ct);
        return product.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, string? name, string? description, 
        decimal? price, decimal? weight, int? categoryId, int? countryId,
        string? imageUrl, bool? isActive, CancellationToken ct = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(u=>u.Id==id, ct);
        if (product == null)
        {
            return false;
        }

        if (name != null)
        {
            product.Name = name;
        }

        if (description != null)
        {
            product.Description = description;
        }

        if (price.HasValue)
        {
            product.Price = price.Value;
        }

        if (weight.HasValue)
        {
            product.Weight = weight.Value;
        }

        if (categoryId.HasValue)
        {
            product.CategoryId = categoryId.Value;
        }

        if (countryId.HasValue)
        {
            product.CountryId = countryId.Value;
        }

        if (imageUrl != null)
        {
            product.ImageUrl = imageUrl;
        }

        if (isActive.HasValue)
        {
            product.IsActive = isActive.Value;
        }
        

        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
    

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = _dbContext.Products.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (product == null)
        {
            return false;
        }
        
        _dbContext.Remove(product);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}