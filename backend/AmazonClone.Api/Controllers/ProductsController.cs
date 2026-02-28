using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonClone.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController: ControllerBase
{
    readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
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

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategoryId(int categoryId)
    {
        return Ok(await _productService.GetByCategoryIdAsync(categoryId));
    }
    [HttpGet("country/{countryId}")]
    public async Task<IActionResult> GetByCountryId(int countryId)
    {
        return Ok(await _productService.GetByCountryIdAsync(countryId));
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

    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommended()
    {
        return Ok(await _productService.GetRecommendedAsync());
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