using AmazonClone.Application.Helpers;
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


    public async Task<bool> CreateDiscountAsync(Guid productId, decimal? discountPrice, int? discountPercentage, DateTime? startDate,
        DateTime? endDate, CancellationToken ct = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p=>p.Id == productId, ct);
        if (product == null)
        {
            return false;
        }

        if (discountPrice == null && discountPercentage == null)
        {
            throw new Exception("Discount price or percentage is required");
        }

        if (discountPrice != null && discountPercentage != null)
        {
            throw new Exception("Only one discount type should be set");
        }

        if (discountPrice.HasValue)
        {
            if (discountPrice <= 0 || discountPrice >= product.Price)
            {
                throw new Exception(
                    "Discount price must be greater than 0 and lower than product price");
            }
        }

        if (discountPercentage.HasValue)
        {
            if (discountPercentage <= 0 || discountPercentage > 100)
            {
                throw new Exception("Discount percentage must be between 1 and 100");
            }
        }

        if (startDate == null && endDate == null)
        {
            throw new Exception("Start date and end date are required");
        }

        if (endDate <= startDate)
        {
            throw new Exception("End date must be greater than start date");
        }

        var existingActiveDiscounts = await _dbContext.Discounts
            .Where(d => d.ProductId == product.Id && d.IsActive)
            .ToListAsync(ct);
        foreach (var existingDiscount in existingActiveDiscounts)
        {
            existingDiscount.IsActive = false;
        }

        var discount = new Discount
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            DiscountPrice = discountPrice,
            DiscountPersentage = discountPercentage,
            StartAt = startDate,
            EndAt = endDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.Discounts.Add(discount);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DisableDiscountAsync(Guid discountId, CancellationToken ct = default)
    {
        var discount = await _dbContext.Discounts.FirstOrDefaultAsync(d=>d.Id == discountId, ct);
        if (discount == null)
        {
            return false;
        }
        
        discount.IsActive = false;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<Product>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Discounts)
            .Where(p => p.IsActive && p.CategoryId == categoryId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        foreach (var product in products)
        {
            DiscountHelper.ApplyDiscount(product);
        }
        
        return products;
    }
    
    public async Task<List<Product>> GetByCountryIdAsync(int countryId, CancellationToken ct = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Discounts)
            .Where(p => p.IsActive && p.CountryId == countryId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        foreach (var product in products)
        {
            DiscountHelper.ApplyDiscount(product);
        }
        
        return products;
    }
}


