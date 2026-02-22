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
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _orderService.CreateAsync(UserId, orderItems);
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(await _orderService.GetMyOrdersAsync(UserId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Order(Guid id)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _orderService.GetByIdAsync(id, UserId);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }
}