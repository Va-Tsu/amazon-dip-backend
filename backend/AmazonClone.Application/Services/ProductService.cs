using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class ProductService : IProductService
{
    readonly ApplicationDbContext _dbContext;

    public ProductService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<Product>> GetAllAsync()
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.Id == id && p.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByCountryIdAsync(int countryId)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.CountryId == countryId)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(string query)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.Name.Contains(query) && p.IsActive)
            .ToListAsync();
    }

    public async Task<List<Product>> GetNewAsync()
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Country)
            .Where(p => p.CreatedAt > DateTime.UtcNow.AddDays(-14))
            .ToListAsync();
    }

    public Task<List<Product>> GetRecommendedAsync()
    {
        throw new NotImplementedException();
    }
}