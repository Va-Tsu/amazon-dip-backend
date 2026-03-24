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
        var newProductId = Guid.NewGuid();

        var newProduct = new Product
        {
            Id = newProductId,
            Name = product.Name,
            Brand = product.Brand,
            Description = product.Description,
            SKU = product.SKU,
            Weight = product.Weight,
            ParcelWeight = product.ParcelWeight,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CountryId = product.CountryId,
            SellerId = product.SellerId,
            Ingridients = product.Ingridients,
            StorageConditions = product.StorageConditions,
            ExpirationDate = product.ExpirationDate,
            Article = product.Article,
            StockQuantity = product.StockQuantity,
            TrackInventory = product.TrackInventory,
            IsActive = product.IsActive,
            CreatedAt = DateTime.UtcNow,
            Images = new List<ProductImage>()
        };

        if (product.Images != null && product.Images.Any())
        {
            var validImages = product.Images
                .Where(i => !string.IsNullOrWhiteSpace(i.Url))
                .ToList();

            for (int i = 0; i < validImages.Count; i++)
            {
                newProduct.Images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = newProductId,
                    Url = validImages[i].Url,
                    SortOrder = i,
                    IsMain = validImages[i].IsMain
                });
            }

            if (newProduct.Images.Count > 0 && !newProduct.Images.Any(i => i.IsMain))
            {
                newProduct.Images.First().IsMain = true;
            }
        }

        _dbContext.Products.Add(newProduct);
        await _dbContext.SaveChangesAsync(ct);

        return newProductId;
    }

    public async Task<bool> UpdateAsync(
    Guid id,
    string? name,
    string? brand,
    string? description,
    string? sku,
    decimal? price,
    decimal? weight,
    decimal? parcelWeight,
    int? categoryId,
    int? countryId,
    string? ingridients,
    string? storageConditions,
    DateTime? expirationDate,
    string? article,
    int? stockQuantity,
    bool? trackInventory,
    List<string>? imageUrls,
    bool? isActive,
    CancellationToken ct = default)
{
    var product = await _dbContext.Products
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p => p.Id == id, ct);

    if (product == null)
        return false;

    if (name != null)
        product.Name = name;

    if (brand != null)
        product.Brand = brand;

    if (description != null)
        product.Description = description;

    if (sku != null)
        product.SKU = sku;

    if (price.HasValue)
        product.Price = price.Value;

    if (weight.HasValue)
        product.Weight = weight.Value;

    if (parcelWeight.HasValue)
        product.ParcelWeight = parcelWeight.Value;

    if (categoryId.HasValue)
        product.CategoryId = categoryId.Value;

    if (countryId.HasValue)
        product.CountryId = countryId.Value;

    if (ingridients != null)
        product.Ingridients = ingridients;

    if (storageConditions != null)
        product.StorageConditions = storageConditions;

    if (expirationDate.HasValue)
        product.ExpirationDate = expirationDate.Value;

    if (article != null)
        product.Article = article;

    if (stockQuantity.HasValue)
        product.StockQuantity = stockQuantity.Value;

    if (trackInventory.HasValue)
        product.TrackInventory = trackInventory.Value;

    if (isActive.HasValue)
        product.IsActive = isActive.Value;

    if (imageUrls != null)
    {
        _dbContext.ProductImages.RemoveRange(product.Images);
        product.Images.Clear();

        var newImages = imageUrls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select((url, index) => new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Url = url,
                SortOrder = index,
                IsMain = index == 0
            })
            .ToList();

        foreach (var image in newImages)
        {
            product.Images.Add(image);
        }
    }

    await _dbContext.SaveChangesAsync(ct);
    return true;
}
    

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (product == null)
        {
            return false;
        }
        
        _dbContext.Remove(product);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}