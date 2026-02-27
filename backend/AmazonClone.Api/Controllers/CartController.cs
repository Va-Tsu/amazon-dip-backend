using System.Security.Claims;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;

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
    
    private const string CartCookie = "cart_id";

    private string GetOrCreateCartKey()
    {
        if (Request.Cookies.TryGetValue(CartCookie, out var key) && !string.IsNullOrWhiteSpace(key))
            return key;

        key = Guid.NewGuid().ToString("N");
        Response.Cookies.Append(CartCookie, key, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,                 // requires HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return key;
    }

    private (string? userId, string cartKey) ResolveOwner()
    {
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        return (userId, GetOrCreateCartKey());
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var (userId, cartKey) = ResolveOwner();
        var cart = await _cartService.GetCartAsync(userId, cartKey, ct);
        return Ok(cart);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddItem(Guid productId, int quantity, CancellationToken ct)
    {
        var (userId, cartKey) = ResolveOwner();
        await _cartService.AddItemAsync(userId, cartKey, productId, quantity, ct);
        return Ok();
    }

    [HttpPut("update/{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] int quantity, CancellationToken ct)
    {
        var (userId, cartKey) = ResolveOwner();
        await _cartService.UpdateItemAsync(userId, cartKey, id, quantity, ct);
        return Ok();
    }

    [HttpDelete("remove/{id:guid}")]
    public async Task<IActionResult> Remove([FromRoute] Guid id, CancellationToken ct)
    {
        var (userId, cartKey) = ResolveOwner();
        await _cartService.RemoveItemAsync(userId, cartKey, id, ct);
        return Ok();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var (userId, cartKey) = ResolveOwner();
        await _cartService.ClearAsync(userId, cartKey, ct);
        return Ok();
    }

    // OPTIONAL: call after login to merge guest cart into user cart
    [Authorize]
    [HttpPost("merge")]
    public async Task<IActionResult> Merge(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartKey = GetOrCreateCartKey();

        await _cartService.MergeGuestCartIntoUserAsync(userId!, cartKey, ct);
        return Ok();
    }

    // OPTIONAL: checkout (choose whether it requires login)
    // If guest checkout is allowed -> remove [Authorize]
    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartKey = GetOrCreateCartKey();

        var orderId = await _cartService.CheckoutAsync(userId, cartKey, ct);
        return Ok(new { orderId });
    }
    
    
}