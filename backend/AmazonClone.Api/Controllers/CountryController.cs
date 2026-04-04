using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonClone.Api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class CountryController: ControllerBase
{
    readonly ICountryService _countryService;

    public CountryController(ICountryService countryService)
    {
        _countryService = countryService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await _countryService.GetAllAsync(ct));
    }
    
    [HttpGet("{id}/products")]
    public async Task<IActionResult> GetProducts(int id,
        string? brand, bool? isAvailable , CancellationToken ct,
        decimal? minPrice = null,  decimal? maxPrice = null, int page = 1,
        int pageSize = 10)
    {
        return Ok(await _countryService.GetProductByCountryIdAsync(id, brand, isAvailable,
             ct, minPrice,  maxPrice, page, pageSize));
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create( Country country, CancellationToken ct)
    {
        var id = await _countryService.CreateAsync(country.Name, country.ImageUrl, country.Code, ct);
        return Ok( new
        {
            message = "A country was created",
            id
        });
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, string? name,
        string? imageUrl, string? code, CancellationToken ct)
    {
        await _countryService.UpdateAsync(id,name, imageUrl,code, ct);
        return Ok( new
        {
            message = "A country was updated",
        });
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _countryService.DeleteAsync(id, ct);
        return Ok( new
        {
            message = "A country was deleted",
        });
    }
}