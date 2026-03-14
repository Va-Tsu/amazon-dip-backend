using System.Security.Claims;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
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
    public async Task<IActionResult> Create( Product product, CancellationToken ct)
    {
        var id = await _productService.CreateAsync(product, ct);
        return CreatedAtAction(nameof(GetById), new { id = id }, product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(Guid id, string? name, string? description,
        decimal? price, decimal? weight, int? categoryId,
        int? countryId, string? imageUrl, bool? isActive, CancellationToken ct)
    {
        var ok = await _productService.UpdateAsync(id, name, description, price,
            weight, categoryId, countryId,
            imageUrl, isActive, ct);
        return ok ? NoContent() : NotFound();
        
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _productService.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
        
    }
}