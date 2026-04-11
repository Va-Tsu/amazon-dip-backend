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
    public async Task<IActionResult> Create( [FromForm] string name,
        [FromForm] IFormFile? image, CancellationToken ct)
    {
        string? imageUrl = null;

        if (image != null && image.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Only .jpg, .jpeg, .png and .webp files are allowed");
            }

            if (!image.ContentType.StartsWith("image/"))
            {
                return BadRequest("The uploaded file must be an image");
            }

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "categories");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream, ct);

            imageUrl = $"/images/categories/{fileName}";
        }
        
        var id = await _categoryService.CreateAsync(name, imageUrl, ct);
        return Ok( new
        {
            message = "A category was created",
            id
        });
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id,
        [FromForm] string? name,
        [FromForm] IFormFile? image, CancellationToken ct)
    {
        string? imageUrl = null;

        if (image != null && image.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Only .jpg, .jpeg, .png and .webp files are allowed");
            }

            if (!image.ContentType.StartsWith("image/"))
            {
                return BadRequest("Uploaded file must be an image");
            }

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "categories");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream, ct);

            imageUrl = $"/images/categories/{fileName}";
        }

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