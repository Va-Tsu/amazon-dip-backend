using AmazonClone.Application.Interfaces;
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
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _countryService.GetAllAsync());
    }
    
    [HttpGet("{id}/products")]
    public async Task<IActionResult> GetProducts(int id, int page = 1, int pageSize = 12,
        decimal? minPrice = null, decimal? maxPrice = null)
    {
        return Ok(await _countryService.GetProductByCountryIdAsync(id, page, pageSize, minPrice, maxPrice));
    }
}