using System.Security.Claims;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Domain.Enums;
using AmazonClone.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController: ControllerBase
{
    readonly IProductService _productService;
    private readonly ApplicationDbContext _dbContext;

    public ProductsController(IProductService productService, ApplicationDbContext dbContext)
    {
        _productService = productService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _productService.GetAllAsync());
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }
    

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        return Ok(await _productService.SearchAsync(query));
    }

    [HttpGet("new")]
    public async Task<IActionResult> GetNew()
    {
        return Ok(await _productService.GetNewAsync());
    }

    [HttpGet("add-recently-viewed/{id}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product == null)
        {
            return NotFound();
        }
        
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        if (!string.IsNullOrEmpty(userId))
        {
            await _productService.AddRecentlyViewedAsync(userId, id, ct);
        }
        return Ok(product);
    }
    
    

    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommended(int page = 1,
        int pageSize = 8, CancellationToken ct = default)
    {
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        var(products, hasMore) = await _productService
            .GetRecommendedAsync(userId, page,pageSize, ct);
        return Ok(new
        {
            page,
            pageSize,
            hasMore,
            products
        });
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create( [FromForm] string name,
        [FromForm] string description,
        [FromForm] string brand,
        [FromForm] decimal weight,
        [FromForm] decimal? parcelWeight,
        [FromForm] string? article,
        [FromForm] decimal price,
        [FromForm] int categoryId,
        [FromForm] int countryId,
        [FromForm] bool trackInventory,
        [FromForm] int stockQuantity,
        [FromForm] int? lowStockTreshold,
        [FromForm] bool isActive,
        [FromForm] bool isPublished,
        [FromForm] string? storageConditions,
        [FromForm] DateTime? expirationDate,
        [FromForm] string? ingridients,
        [FromForm] string sku,
        [FromForm] decimal? oldPrice,
        [FromForm] bool hasDiscount,
        [FromForm] Guid? sellerId,
        [FromForm] ProductStatus status,
        [FromForm] List<IFormFile>? images,
        CancellationToken ct)
    {
        var imageUrls = new List<string>();

        if (images != null && images.Count > 0)
        {
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "products");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var image in images)
            {
                if (image == null || image.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only .jpg, .jpeg, .png and .webp files are allowed");
                }

                if (!image.ContentType.StartsWith("image/"))
                {
                    return BadRequest("Uploaded file must be an image");
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream, ct);

                imageUrls.Add($"/images/products/{fileName}");
            }
        }
        
        var product = new Product
        {
            Name = name,
            Description = description,
            Brand = brand,
            Weight = weight,
            ParcelWeight = parcelWeight,
            Article = article,
            Price = price,
            CategoryId = categoryId,
            CountryId = countryId,
            TrackInventory = trackInventory,
            StockQuantity = stockQuantity,
            LowStockTreshold = lowStockTreshold,
            CreatedAt = DateTime.UtcNow,
            IsActive = isActive,
            IsPublished = isPublished,
            StorageConditions = storageConditions,
            ExpirationDate = expirationDate,
            Ingridients = ingridients,
            SKU = sku,
            OldPrice = oldPrice,
            HasDiscount = hasDiscount,
            SellerId = sellerId,
            Status = status
        };
        
        var id = await _productService.CreateAsync(product,imageUrls, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update( Guid id,
        [FromForm] string? name,
        [FromForm] string? brand,
        [FromForm] string? description,
        [FromForm] string? sku,
        [FromForm] decimal? price,
        [FromForm] decimal? weight,
        [FromForm] decimal? parcelWeight,
        [FromForm] int? categoryId,
        [FromForm] int? countryId,
        [FromForm] string? ingridients,
        [FromForm] string? storageConditions,
        [FromForm] DateTime? expirationDate,
        [FromForm] string? article,
        [FromForm] int? stockQuantity,
        [FromForm] int? lowStockTreshold,
        [FromForm] bool? trackInventory,
        [FromForm] decimal? currentPrice,
        [FromForm] decimal? oldPrice,
        [FromForm] bool? hasDiscount,
        [FromForm] bool? isActive,
        [FromForm] bool? isPublished,
        [FromForm] Guid? sellerId,
        [FromForm] ProductStatus? status,
        [FromForm] List<IFormFile>? images,
        CancellationToken ct)
    {
        var savedImageUrls = new List<string>();

        if (images != null && images.Count > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "products");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var image in images)
            {
                if (image == null || image.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only .jpg, .jpeg, .png and .webp files are allowed");
                }

                if (!image.ContentType.StartsWith("image/"))
                {
                    return BadRequest("The uploaded file must be an image");
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream, ct);

                savedImageUrls.Add($"/images/products/{fileName}");
            }
        }

        var ok = await _productService.UpdateAsync(
            id,
            name,
            brand,
            description,
            sku,
            weight,
            parcelWeight,
            categoryId,
            countryId,
            ingridients,
            storageConditions,
            expirationDate,
            article,
            stockQuantity,
            lowStockTreshold,
            trackInventory,
            price,
            hasDiscount,
            savedImageUrls,
            isActive,
            isPublished,
            sellerId,
            status,
            ct);

        if (!ok)
            return NotFound();

        return Ok("Product updated");
        
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _productService.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
        
    }
}