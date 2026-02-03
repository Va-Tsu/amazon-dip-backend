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
    Task<List<Product>> GetRecommendedAsync();

}