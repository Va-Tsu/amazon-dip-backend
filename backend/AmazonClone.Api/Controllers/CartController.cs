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


    [Authorize]
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(await _cartService.GetMyCartAsync(identityUserId));
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddItem(CartItem cartItem)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.AddItemAsync(identityUserId, cartItem.ProductId, cartItem.Quantity);
        return Ok();
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(Guid itemId, int quantity)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.UpdateItemAsync(identityUserId, itemId, quantity);
        return Ok();
        
    }

    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> Remove(Guid itemId)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.RemoveItemAsync(identityUserId, itemId);
        return Ok();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.ClearAsync(identityUserId);
        return Ok();
    }
}