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

    public async Task<Guid> CreateAsync(Product product, CancellationToken ct = default)
    {
        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = product.Name,
            Description = product.Description,
            Weight = product.Weight,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CountryId = product.CountryId,
            CreatedAt = DateTime.Now,
            ImageUrl = product.ImageUrl
        };
        _dbContext.Products.Add(newProduct);
        await _dbContext.SaveChangesAsync(ct);
        return product.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, string? name, string? description, 
        decimal? price, decimal? weight, int? categoryId, int? countryId,
        string? imageUrl, bool? isActive, CancellationToken ct = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(u=>u.Id==id, ct);
        if (product == null)
        {
            return false;
        }

        if (name != null)
        {
            product.Name = name;
        }

        if (description != null)
        {
            product.Description = description;
        }

        if (price.HasValue)
        {
            product.Price = price.Value;
        }

        if (weight.HasValue)
        {
            product.Weight = weight.Value;
        }

        if (categoryId.HasValue)
        {
            product.CategoryId = categoryId.Value;
        }

        if (countryId.HasValue)
        {
            product.CountryId = countryId.Value;
        }

        if (imageUrl != null)
        {
            product.ImageUrl = imageUrl;
        }

        if (isActive.HasValue)
        {
            product.IsActive = isActive.Value;
        }
        

        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
    

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = _dbContext.Products.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (product == null)
        {
            return false;
        }
        
        _dbContext.Remove(product);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}