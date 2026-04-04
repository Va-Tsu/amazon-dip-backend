using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICountryService
{
    Task<int> CreateAsync(string name, string imageUrl, string? code, CancellationToken ct);
    Task UpdateAsync(int id, string? name, string? imageUrl, string? code, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<List<Product>> GetProductByCountryIdAsync(int countryId, string? brand,
        bool? isAvailable, CancellationToken ct, decimal? minPrice = null, decimal? maxPrice = null, int page = 1,
        int pageSize = 10);
    Task<List<Country>> GetAllAsync(CancellationToken ct);
}