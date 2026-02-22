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
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(await _cartService.GetMyCartAsync(UserId));
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddItem(CartItem cartItem)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.AddItemAsync(UserId, cartItem.ProductId, cartItem.Quantity);
        return Ok();
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(Guid itemId, int quantity)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.UpdateItemAsync(UserId, itemId, quantity);
        return Ok();
        
    }

    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> Remove(Guid itemId)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.RemoveItemAsync(UserId, itemId);
        return Ok();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _cartService.ClearAsync(UserId);
        return Ok();
    }
}