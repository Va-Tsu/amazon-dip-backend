using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Application.Services;

public class CartService : ICartService
{
    readonly ApplicationDbContext _dbContext;

    public CartService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<Cart> GetCartAsync(string? userId, string cartKey, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(userId, cartKey, ct);

        if (cart == null)
            return new Cart { Items = new(), TotalPrice = 0 };

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
            TotalPrice = items.Sum(i => i.Price * i.Quantity) 
        };
    }

    

    public async Task AddItemAsync(string? userId, string cartKey, Guid productId, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var product = await _dbContext.Products.FirstOrDefaultAsync(p=>p.Id==productId, ct)
                      ?? throw new Exception("Product not found");

        var cart = await GetOrCreateCartAsync(userId, cartKey, ct);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductName = product.Name,
                Quantity = quantity,
                Price = product.Price,
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("Cart was modified by another request. Please retry.");
        }
        
    }

    public async Task UpdateItemAsync(string? userId, string cartKey, Guid cartItemId, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var item = await _dbContext.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == cartItemId, ct)
            ?? throw new Exception("Item wasn't found");

        EnsureOwnership(item.Cart, userId, cartKey);

        item.Quantity = quantity;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveItemAsync(string? userId, string cartKey, Guid cartItemId, CancellationToken ct = default)
    {
        var item = await _dbContext.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == cartItemId, ct)
            ?? throw new Exception("Item wasn't found");

        EnsureOwnership(item.Cart, userId, cartKey);

        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ClearAsync(string? userId, string cartKey, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(userId, cartKey, ct);
        if (cart == null) return;

        _dbContext.CartItems.RemoveRange(cart.Items);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task MergeGuestCartIntoUserAsync(string userId, string cartKey, CancellationToken ct = default)
    {
        // guest cart by cartKey
        var guest = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CartKey == cartKey && c.UserId == null, ct);

        if (guest == null || guest.Items.Count == 0) return;

        // user cart by userId
        var userCart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (userCart == null)
        {
            // simply "claim" the guest cart
            guest.UserId = userId;
            await _dbContext.SaveChangesAsync(ct);
            return;
        }

        // merge items
        foreach (var gItem in guest.Items)
        {
            var existing = userCart.Items.FirstOrDefault(i => i.ProductId == gItem.ProductId);
            if (existing != null) existing.Quantity += gItem.Quantity;
            else userCart.Items.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = gItem.ProductId,
                ProductName = gItem.ProductName,
                Price = gItem.Price,
                Quantity = gItem.Quantity
            });
        }

        // remove guest cart (or clear it)
        _dbContext.CartItems.RemoveRange(guest.Items);
        _dbContext.Carts.Remove(guest);

        await _dbContext.SaveChangesAsync(ct);
    }

    // -------- helpers --------

    private async Task<Cart?> FindCartAsync(string? userId, string cartKey, CancellationToken ct)
    {
        // If logged in, prefer user cart
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        }

        // Guest cart by key
        return await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CartKey ==cartKey && c.UserId == null, ct);
    }

    private async Task<Cart> GetOrCreateCartAsync(string? userId, string cartKey, CancellationToken ct)
    {
        var cart = await FindCartAsync(userId, cartKey, ct);
        if (cart != null) return cart;

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            CartKey = string.IsNullOrWhiteSpace(userId) ? cartKey:null,
            Items = new List<CartItem>()
        };

        _dbContext.Carts.Add(cart);
        return cart;
    }

    private static void EnsureOwnership(Cart cart, string? userId, string cartKey)
    {
        // If cart belongs to a user -> require same user
        if (!string.IsNullOrWhiteSpace(cart.UserId))
        {
            if (cart.UserId != userId)
                throw new UnauthorizedAccessException("Not your cart item.");
            return;
        }

        // Guest cart -> require same cartKey
        if (!string.Equals(cart.CartKey, cartKey, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your guest cart item.");
    }
    
    public async Task<Guid> CheckoutAsync(string? userId, string cartKey, CancellationToken ct = default)
{
    // Load cart
    var cart = await _dbContext.Carts
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c =>
            (!string.IsNullOrWhiteSpace(userId) && c.UserId == userId) ||
            (string.IsNullOrWhiteSpace(userId) && c.UserId == null && c.CartKey == cartKey),
            ct);

    if (cart == null || cart.Items.Count == 0)
        throw new Exception("Cart is empty");

    // OPTIONAL: validate products/stock/fresh price
    // Load products for all cart items in one query
    var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();

    var products = await _dbContext.Products
        .Where(p => productIds.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id, ct);

    // Validate all items exist
    foreach (var item in cart.Items)
        if (!products.ContainsKey(item.ProductId))
            throw new Exception($"Product {item.ProductId} not found");

    await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

    // Create order
    var order = new Order
    {
        Id = Guid.NewGuid(),
        UserId = userId,        // null if guest
        CartKey = cartKey,      // optional
        CreatedAt = DateTime.UtcNow,
        Items = new List<OrderItem>()
    };

    // Create order items (SNAPSHOT)
    foreach (var cartItem in cart.Items)
    {
        var p = products[cartItem.ProductId];

        order.Items.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = p.Id,
            ProductName = p.Name,
            Price = p.Price,          // snapshot price at purchase time
            Quantity = cartItem.Quantity
        });
    }

    order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity);

    _dbContext.Orders.Add(order);

    // Clear cart
    _dbContext.CartItems.RemoveRange(cart.Items);

    // Optional: keep cart row for reuse; or delete it:
    // _dbContext.Carts.Remove(cart);

    await _dbContext.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    return order.Id;
}
 

    
}