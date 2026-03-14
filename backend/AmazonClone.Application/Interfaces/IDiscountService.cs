using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface IDiscountService
{
    Task<bool> CreateDiscountAsync(Guid productId, decimal? discountPrice,
        int? discountPercentage, DateTime? startDate, DateTime? endDate, CancellationToken ct = default);

    Task<bool> DisableDiscountAsync(Guid discountId,  CancellationToken ct = default);
    Task<List<Product>> GetByCountryIdAsync(int countryId, CancellationToken ct = default);
    Task<List<Product>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
}