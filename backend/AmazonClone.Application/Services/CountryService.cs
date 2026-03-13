using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class CountryService:ICountryService
{
    readonly ApplicationDbContext _dbContext;

    public CountryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Product>> GetProductByCountryIdAsync(int countryId, int page = 1, int pageSize = 10, decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var query = _dbContext.Products.Where(p=>p.CategoryId==countryId);
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

    public async Task<List<Country>> GetAllAsync()
    {
        return await _dbContext.Countries.ToListAsync();
    }
}