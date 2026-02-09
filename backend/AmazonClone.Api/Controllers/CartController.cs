using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AmazonClone.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    readonly ICartService _cartService;
    readonly UserManager<IdentityUser> _userManager;

    public CartController(ICartService cartService, UserManager<IdentityUser> userManager)
    {
        _cartService = cartService;
        _userManager = userManager;
    }


    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetCart()
    {
        var identityUserId = (await _userManager.GetUserAsync(User))?.Id;
        var cart = await _cartService.GetOrCreateCartAsync(identityUserId);
        return Ok(cart);
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
    {
        var identityUserId = (await _userManager.GetUserAsync(User))?.Id;
        await _cartService.AddProductAync(identityUserId, productId, quantity);
        return Ok();
    }
}