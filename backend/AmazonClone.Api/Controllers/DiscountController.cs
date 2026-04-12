using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonClone.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class DiscountController: ControllerBase
{
    readonly IDiscountService _discountService;

    public DiscountController(IDiscountService discountService)
    {
        _discountService = discountService;
    }
    
    [HttpGet("category{categoryId}")]
    public async Task<IActionResult> GetByCategoryId(int categoryId, CancellationToken ct = default)
    {
        var products = await _discountService.GetByCategoryIdAsync(categoryId, ct);
        return Ok(products);
    }
    
    [HttpGet("country{countryId}")]
    public async Task<IActionResult> GetByCountryId(int countryId, CancellationToken ct = default)
    {
        var products = await _discountService.GetByCategoryIdAsync(countryId, ct);
        return Ok(products);
    }
}