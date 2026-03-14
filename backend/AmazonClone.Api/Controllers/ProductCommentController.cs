using System.Security.Claims;
using AmazonClone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonClone.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ProductCommentController:ControllerBase
{
    readonly IProductCommentService _productCommentService;

    public ProductCommentController(IProductCommentService productCommentService)
    {
        _productCommentService = productCommentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProductId(Guid productId, CancellationToken ct = default)
    {
        var comments = await _productCommentService
            .GetByProductIdAsync(productId, ct);
        return Ok(comments);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(Guid productId, string text,
        int rating, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var comment = await _productCommentService.AddAsync(userId, productId, text, rating, ct);
        if (comment == null)
        {
            return NotFound("Product isn't found");
        }
        return Ok(comment);
    }

    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> Delete(Guid commentId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        var ok = await _productCommentService.DeleteAsync(userId, commentId, ct);
        if (!ok)
        {
            return NotFound("Comment isn't found or access is denied");
        }
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{commentId}/admin")]
    public async Task<IActionResult> InternalDelete(Guid commentId, CancellationToken ct = default)
    {
        var ok = await _productCommentService.InternalDeleteAsync(commentId, ct);
        if (!ok)
        {
            return NotFound("Comment isn't found");
        }
        return NoContent();
    }
    
}