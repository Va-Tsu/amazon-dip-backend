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

    public async Task<int> CreateAsync(string name, string? imageUrl, CancellationToken ct)
    {
        var exists = await _dbContext.Categories
            .AnyAsync(c => c.Name == name, ct);
        if (exists)
        {
            throw new Exception($"Category with name {name} already exists");
        }

        var category = new Category()
        {
            Name = name,
            ImageUrl = imageUrl
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(ct);

        return category.Id;
    }

    public async Task UpdateAsync(int id, string? name, string? imageUrl, CancellationToken ct)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category == null)
        {
            throw new Exception($"Category with id {id} not found");
        }
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            var exists = await _dbContext.Categories
                .AnyAsync(c => c.Name == name && c.Id != id, ct);
            if (exists)
            {
                throw new Exception($"Category with name {name} already exists");
            }
            category.Name = name;
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            category.ImageUrl = imageUrl;
        }
        
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category == null)
        {
            throw new Exception($"Category with id {id} not found");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken ct)
    {
        return await _dbContext.Categories.ToListAsync();
    }

   

    public async Task<List<Product>> GetProductByCategoryIdAsync(int categoryId,
        string? brand, bool? isAvailable, CancellationToken ct, decimal? minPrice = null,
        decimal? maxPrice = null, int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Products.AsQueryable();
        query = _dbContext.Products.Where(p => p.CategoryId == categoryId);

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

        query = query.Skip((page - 1) * pageSize).Take(pageSize);
        return await query.ToListAsync();

    }
}
    
    