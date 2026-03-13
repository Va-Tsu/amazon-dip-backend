using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICountryService
{
    Task<List<Product>> GetProductByCountryIdAsync(int countryId, int page = 1, int pageSize = 10,
        decimal? minPrice = null, decimal? maxPrice = null);
    Task<List<Country>> GetAllAsync();
}