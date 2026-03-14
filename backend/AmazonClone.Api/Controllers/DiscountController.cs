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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Create(Guid productId,
        decimal? discountPrice,
        int? discountPercentage,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken ct = default)
    {
        var ok = await _discountService.CreateDiscountAsync(
            productId,
            discountPrice,
            discountPercentage,
            startDate,
            endDate,
            ct);

        if (!ok)
        {
            return NotFound("Product isn't found");
        }
        return Ok("Discount created");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{discountId}/disable")]
    public async Task<IActionResult> Disable(Guid discountId,
        CancellationToken ct = default)
    {
        var ok = await _discountService.DisableDiscountAsync(discountId, ct);
        if (!ok)
        {
            return NotFound("Discount isn't found");
        }
        
        return Ok("Discount disabled");
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