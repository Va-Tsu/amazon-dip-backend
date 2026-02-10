using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class CategoryService : ICategoryService
{
    readonly ApplicationDbContext _dbContext;

    public CategoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _dbContext.Categories.ToListAsync();
    }
public async Task<List<Product>> GetProductByCategoryIdAsync(int categoryId, int page = 1, int pageSize = 12, decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var query = _dbContext.Products.Where(p=>p.CategoryId==categoryId);
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }
        
        query = query.Skip((page-1)*pageSize).Take(pageSize);
        return await query.ToListAsync();

    }
    
}