using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetByCategoryIdAsync(int categoryId);
    Task<List<Product>> GetByCountryIdAsync(int countryId);
    Task<List<Product>> SearchAsync(string query);
    Task<List<Product>> GetNewAsync();
    
    Task<(List<Product>, bool hasMore)> GetRecommendedAsync(string? userId, int page,int pageSize, CancellationToken ct = default);
    Task AddRecentlyViewedAsync(string userId, Guid productId, CancellationToken ct = default);
    Task<Guid> CreateAsync(Product product, CancellationToken ct = default);
    
    Task<bool> UpdateAsync(Guid id, string? name, string? description,
        decimal? price, decimal? weight, int? categoryId,
        int? countryId, string? imageUrl, bool? isActive , CancellationToken ct = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    
    

}