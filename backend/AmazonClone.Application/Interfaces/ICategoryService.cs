using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICategoryService
{
    Task<int> CreateAsync(string name, string? imageUrl, CancellationToken ct);
    Task UpdateAsync(int id, string? name, string? imageUrl, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<List<Category>> GetAllAsync(CancellationToken ct);
    Task<List<Product>> GetProductByCategoryIdAsync(int categoryId, string? brand ,
        bool? isAvailable, CancellationToken ct, decimal? minPrice = null,
        decimal? maxPrice = null, int page = 1, int pageSize = 10);
    
}