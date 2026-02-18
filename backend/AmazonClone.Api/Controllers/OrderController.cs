using System.Security.Claims;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AmazonClone.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    

    [HttpPost("create")]
    public async Task<ActionResult> Create(List<OrderItem> orderItems)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _orderService.CreateAsync(identityUserId, orderItems);
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(await _orderService.GetMyOrdersAsync(identityUserId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Order(Guid id)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _orderService.GetByIdAsync(id, identityUserId);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }
}