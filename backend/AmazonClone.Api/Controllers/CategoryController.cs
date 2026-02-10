using AmazonClone.Application.Interfaces;
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
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _categoryService.GetAllAsync());
    }

    [HttpGet("{id}/products")]
    public async Task<IActionResult> GetProducts(int id, int page = 1, int pageSize = 12,
        decimal? minPrice = null, decimal? maxPrice = null)
    {
        return Ok(await _categoryService.GetProductByCategoryIdAsync(id, page, pageSize, minPrice, maxPrice));
    }
}