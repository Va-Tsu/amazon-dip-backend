using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface IDiscountService
{
    Task<List<Product>> GetByCountryIdAsync(int countryId, CancellationToken ct = default);
    Task<List<Product>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
}