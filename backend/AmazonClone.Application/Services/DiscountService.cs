using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class DiscountService : IDiscountService
{
    private readonly ApplicationDbContext _dbContext;

    public DiscountService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    

    public async Task<List<Product>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.CategoryId == categoryId && p.HasDiscount == true)
            .Include(p=>p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
        
        return products;
    }
    
    public async Task<List<Product>> GetByCountryIdAsync(int countryId, CancellationToken ct = default)
    {
        var products = await _dbContext.Products
            .Where(p => p.IsActive && p.CountryId == countryId && p.HasDiscount == true)
            .OrderByDescending(p => p.CreatedAt)
            .Include(p=>p.Images)
            .ToListAsync(ct);
        
        return products;
    }
}


