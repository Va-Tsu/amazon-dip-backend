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

    public async Task<int> CreateAsync(string name, string? imageUrl, string? code, CancellationToken ct)
    {
        var exists = await _dbContext.Countries
            .AnyAsync(c=>c.Name == name , ct);
        if (exists)
        {
            throw new Exception($"Country with name {name} already exists");
        }

        var country = new Country()
        {
            Name = name,
            ImageUrl = imageUrl,
            Code = code
        };
        
        _dbContext.Countries.Add(country);
        await _dbContext.SaveChangesAsync(ct);

        return country.Id;
    }

    public async Task UpdateAsync(int id, string? name, string? imageUrl, string? code, CancellationToken ct)
    {
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (country == null)
        {
            throw new Exception($"Country with id {id} not found");
        }
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            var exists = await _dbContext.Countries
                .AnyAsync(c => c.Name == name && c.Id!=id, ct);
            if (exists)
            {
                throw new Exception($"Country with name {name} already exists");
            }
        
            country.Name = name;
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            country.ImageUrl = imageUrl;
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            country.Code = code;
        }
        
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (country == null)
        {
            throw new Exception($"Country with id {id} not found");
        }
        
        _dbContext.Countries.Remove(country);
        await _dbContext.SaveChangesAsync(ct);
    }
    

    public async Task<List<Product>> GetProductByCountryIdAsync(int countryId,
        string? brand, bool? isAvailable, CancellationToken ct, decimal? minPrice = null,
        decimal? maxPrice = null,  int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Products.AsQueryable();
        query = _dbContext.Products.Where(p=>p.CountryId==countryId);

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand == brand);
        }
        
        if (isAvailable.HasValue)
        {
            if (isAvailable.Value)
                query = query.Where(p => p.StockQuantity > 0);
            else
                query = query.Where(p => p.StockQuantity == 0);
        }
        
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

    public async Task<List<Country>> GetAllAsync(CancellationToken ct)
    {
        return await _dbContext.Countries.ToListAsync();
    }
}