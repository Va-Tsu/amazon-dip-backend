using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonClone.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await _categoryService.GetAllAsync(ct));
    }

    [HttpGet("{id}/products")]
    public async Task<IActionResult> GetProducts(int id,
        string? brand, bool? isAvailable ,CancellationToken ct,  decimal? minPrice = null,
        decimal? maxPrice = null, int page = 1,
        int pageSize = 10)
    {
        return Ok(await _categoryService.GetProductByCategoryIdAsync(id, brand, isAvailable,
             ct,  minPrice,  maxPrice,  page, pageSize));
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create( Category category, CancellationToken ct)
    {
        var id = await _categoryService.CreateAsync(category.Name, category.ImageUrl, ct);
        return Ok( new
        {
            message = "A category was created",
            id
        });
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, string? name, string? imageUrl, CancellationToken ct)
    {
        await _categoryService.UpdateAsync(id,name, imageUrl, ct);
        return Ok( new
        {
            message = "A category was updated",
        });
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(id, ct);
        return Ok( new
        {
            message = "A category was deleted",
        });
    }
}