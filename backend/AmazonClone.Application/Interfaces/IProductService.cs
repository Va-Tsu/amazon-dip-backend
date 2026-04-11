using AmazonClone.Domain.Entities;
using AmazonClone.Domain.Enums;

namespace AmazonClone.Application.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<List<Product>> SearchAsync(string query);
    Task<List<Product>> GetNewAsync();
    
    Task<(List<Product>, bool hasMore)> GetRecommendedAsync(string? userId, int page,
        int pageSize, CancellationToken ct = default);
    Task AddRecentlyViewedAsync(string userId, Guid productId, CancellationToken ct = default);
    Task<Guid> CreateAsync(Product product,List<string> imageUrls, CancellationToken ct = default);

    Task<bool> UpdateAsync(
        Guid id, string? name, string? brand, string? description, string? sku, 
        decimal? weight, decimal? parcelWeight, int? categoryId,
        int? countryId, string? ingridients, string? storageConditions, 
        DateTime? expirationDate, string? article, int? stockQuantity, 
        int? lowStockTreshold, bool? trackInventory,
        decimal? price, bool? hasDiscount, List<string>? imageUrls,
        bool? isActive, bool? isPublished, Guid? sellerId, ProductStatus? status,
        CancellationToken ct);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    
    

}