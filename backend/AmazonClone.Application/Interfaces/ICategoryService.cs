using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<List<Product>> GetProductByCategoryIdAsync(int categoryId, int page = 1, int pageSize = 10,
        decimal? minPrice = null, decimal? maxPrice = null);
    
}