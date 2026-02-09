using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class CartService:ICartService
{
    readonly ApplicationDbContext _dbContext;

    public CartService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Cart?> GetOrCreateCartAsync(string identityUserId)
    {
        var cart = await _dbContext.Carts
            .Include(c=>c.Items)
            .ThenInclude(i=>i.Product)
            .FirstOrDefaultAsync(c=>c.IdentityUserId == identityUserId);

        
        if (cart == null)
        {
            cart = new Cart { IdentityUserId = identityUserId };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();
        }

        return cart;
    }

    public async Task AddProductAync(string identityUserId, Guid productId, int quantity)
    {
        var cart = await GetOrCreateCartAsync(identityUserId);
        
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem{ProductId = productId,
                Quantity = quantity,
                CartId = cart.Id});
        }

        await _dbContext.SaveChangesAsync();
    }
}