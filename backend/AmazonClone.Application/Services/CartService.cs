using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class CartService: ICartService
{
    readonly ApplicationDbContext _dbContext;

    public CartService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<Cart?> GetMyCartAsync(string identityUserId)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c=>c.IdentityUserId == identityUserId);
        if (cart == null)
        {
            return new Cart
            {
                Items = new(),
                TotalPrice = 0
            };
        }

        var items = cart.Items.Select(i => new CartItem
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Price = i.Price,
            Quantity = i.Quantity
        }).ToList();
        
        return new Cart
        {
            Items = items,
            TotalPrice = items.Sum(i=>i.Price)
        };
    }

    public async Task UpdateItemAsync(string identityUserId, Guid cartItemId, int quantity)
    {
       var item = await _dbContext.CartItems
           .Include(i=>i.Cart)
           .FirstOrDefaultAsync(i=>i.Id==cartItemId && i.Cart.IdentityUserId==identityUserId)
           ?? throw new Exception("Item wasn't found");
       
       item.Quantity = quantity;
       await _dbContext.SaveChangesAsync();
       
    }

    public async Task RemoveItemAsync(string identityUserId, Guid cartItemId)
    {
        var item = await _dbContext.CartItems
                       .Include(i=>i.Cart)
                       .FirstOrDefaultAsync(i=>i.Id==cartItemId && i.Cart.IdentityUserId==identityUserId)
                   ?? throw new Exception("Item wasn't found");
       
        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ClearAsync(string identityUserId)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c=>c.IdentityUserId == identityUserId);

        if (cart == null)
        {
            return;
        }
        _dbContext.CartItems.RemoveRange(cart.Items);
        await _dbContext.SaveChangesAsync();
    }
    public async Task AddItemAsync(string identityUserId, Guid productId, int quantity)
    {
        var product = await _dbContext.Products.FindAsync(productId)
                      ?? throw new Exception("Product not found");
        var cart = await GetOrCreateCartAsync(identityUserId);
        
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem{
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductName = product.Name,
                Quantity = quantity,
                Price = product.Price,
                });
        }

        await _dbContext.SaveChangesAsync();
    }


    private async Task<Cart?> GetOrCreateCartAsync(string identityUserId)
    {
        var cart = await _dbContext.Carts
            .Include(c=>c.Items)
            .FirstOrDefaultAsync(c=>c.IdentityUserId == identityUserId);

        
        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                IdentityUserId = identityUserId,
                Items = new List<CartItem>()
            };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();
        }

        return cart;
    }

    
}