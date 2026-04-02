using System.Security.Claims;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Domain.Enums;
using AmazonClone.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SellerController:ControllerBase
{
    readonly ApplicationDbContext _dbContext;
    readonly UserManager<User> _userManager;
    readonly SignInManager<User> _signInManager;
    readonly IProductService _productService;

    public SellerController(ApplicationDbContext dbContext,
        UserManager<User> userManager, SignInManager<User> signInManager,
        IProductService productService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _productService = productService;

    }
    
    [HttpPost("register")]
    public async Task<ActionResult> RegisterSeller(string fullName,
        string email, string password, string confirmedPassword,
        string? phoneNumber, string storeName, string country, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (password != confirmedPassword)
            {
                return BadRequest("Passwords do not match");
            }

            if (user == null)
            {
                
                return BadRequest("User with such email doesn't exist");
                
            }
            
            var check = await _signInManager.
                CheckPasswordSignInAsync(user, password, false);
            if (!check.Succeeded)
            {
                return Unauthorized("Invalid credentials");
            }
            
            var existingSeller = await _dbContext.Sellers
                .FirstOrDefaultAsync(s=>s.UserId == user.Id, ct);
            if (existingSeller != null)
            {
                return BadRequest("Seller account already exists");
            }
            /*var countryExists = await _dbContext.Countries
                .AnyAsync(c => c.Id == countryId, ct);
            if (!countryExists)
            {
                return BadRequest("Invalid country");
            }*/

            var seller = new Seller()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                StoreName = storeName,
                FullName = fullName,
                Country = country,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Sellers.Add(seller);
            await _dbContext.SaveChangesAsync(ct);
            await _userManager.AddToRoleAsync(user, "Seller");
            return Ok(new
            {
                message = "Seller account created",
                sellerId = seller.Id
            });

        }

    
    [Authorize]
    [HttpPost("login")]
    public async Task<IActionResult> LoginSeller(string fullName, string email, string password, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email and password are required");

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Entered email does not match current user");

        if (!string.IsNullOrWhiteSpace(fullName) &&
            !string.Equals(user.FullName, fullName, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Entered full name does not match current user");

        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!passwordCheck.Succeeded)
            return Unauthorized("Invalid password");

        var seller = await _dbContext.Sellers
            .AsNoTracking()
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.UserId == user.Id, ct);

        var roles = await _userManager.GetRolesAsync(user);
        var hasSellerRole = roles.Contains("Seller");

        if (seller == null || !hasSellerRole)
            return BadRequest("Seller account not found");

        return Ok(new
        {
            message = "Seller access granted",
            seller = new
            {
                seller.Id,
                seller.StoreName,
                seller.Description,
                seller.ImageUrl,
                seller.IsActive,
                seller.Balance,
                seller.PandingBalance,
                seller.CreatedAt,
                /*country = seller.Country == null ? null : new
                {
                    seller.Country.Id,
                    seller.Country.Name,
                    seller.Country.Code
                }*/
                seller.Country
            }
        });
    }   
    

    [Authorize(Roles = "Seller")]
    [HttpGet("me")]
    public async Task<IActionResult> GetSellerDashboard(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        var seller  = await _dbContext.Sellers
            .AsNoTracking()
            .Include(s=>s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (seller == null)
        {
            return NotFound("Seller not found");
        }
        var activeProductsCount = await _dbContext.Products
            .AsNoTracking()
            .CountAsync(p=>p.SellerId==seller.Id && p.IsActive,ct);
        var myProducts = await _dbContext.Products
            .AsNoTracking()
            .Include(p=>p.Images)
            .Where(p=>p.SellerId==seller.Id)
            .OrderByDescending(p=>p.CreatedAt)
            .Take(10)
            .ToListAsync(ct);
        var newOrdersCount = await _dbContext.OrderItems
            .Where(oi => oi.Product.SellerId==seller.Id && oi.Order.Status==OrderStatus.New)
            .Select(oi => oi.OrderId)
            .Distinct()
            .CountAsync(ct);
        var pendingOrdersCount = await _dbContext.OrderItems
            .Where(oi => oi.Product.SellerId==seller.Id && oi.Order.Status==OrderStatus.Pending)
            .Select(oi => oi.OrderId)
            .Distinct()
            .CountAsync(ct);
        var sells = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(oi=>oi.Product.SellerId==seller.Id)
            .SumAsync(oi=> (decimal?)oi.Price *oi.Quantity, ct)??0m;
        
        var now = DateTime.UtcNow;
        var dayStart = now.Date;
        var weekStart = dayStart.AddDays(-((int)dayStart.DayOfWeek == 0 ? 6 : (int)dayStart.DayOfWeek - 1));
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var revenueDay = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(oi =>
                oi.Product.SellerId == seller.Id &&
                (oi.Order.Status==OrderStatus.Delivered || oi.Order.Status== OrderStatus.Completed) &&
                oi.Order.CreatedAt >= dayStart)
            .SumAsync(oi => (decimal?)oi.Price * oi.Quantity, ct) ?? 0m;

        var revenueWeek = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(oi =>
                oi.Product.SellerId == seller.Id &&
                (oi.Order.Status==OrderStatus.Delivered || oi.Order.Status == OrderStatus.Completed) &&
                oi.Order.CreatedAt >= weekStart)
            .SumAsync(oi => (decimal?)oi.Price * oi.Quantity, ct) ?? 0m;

        var revenueMonth = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(oi =>
                oi.Product.SellerId == seller.Id &&
                (oi.Order.Status==OrderStatus.Delivered || oi.Order.Status==OrderStatus.Completed) &&
                oi.Order.CreatedAt >= monthStart)
            .SumAsync(oi => (decimal?)oi.Price * oi.Quantity, ct) ?? 0m;
        
        return Ok(new
        {
            seller = new
            {
                seller.Id,
                seller.StoreName,
                seller.Description,
                seller.ImageUrl,
                seller.IsActive,
                seller.Balance,
                seller.PandingBalance,
                seller.CreatedAt,
                seller.Country,
                /*country= seller.Country == null ? null: new
                {
                    seller.Country.Id,
                    seller.Country.Name,
                    seller.Country.Code
                },*/
                user = seller.User == null? null : new 
                {
                    seller.User.Id,
                    seller.User.FullName,
                    seller.User.Email,
                    seller.User.PhoneNumber,
                    seller.User.ImageUrl
                }
            },
            stats = new
            {
                activeProductsCount,
                newOrdersCount,
                pendingOrdersCount,
                sells,
                balance = seller.Balance,
                pendingBalance = seller.PandingBalance,
                revenue = new
                {
                    day = revenueDay,
                    week = revenueWeek,
                    month = revenueMonth
                }
            },
            products = myProducts.Select(p=> new
            {
                p.Id,
                p.Name,
                p.Brand,
                p.Price,
                p.CurrentPrice,
                p.OldPrice,
                p.HasDiscount,
                p.IsActive,
                p.CreatedAt,
                mainImage = p.Images
                    .OrderBy(i=>i.SortOrder)
                    .Select(i=>i.Url)
                    .FirstOrDefault()
            })
        });
    }
    [Authorize(Roles = "Seller")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var seller = await _dbContext.Sellers
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

        if (seller == null)
            return NotFound("Seller account not found");

        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == seller.Id, ct);

        if (product == null)
            return NotFound();

        return Ok(product);
    }
    
    [Authorize(Roles = "Seller")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] Product product, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var seller = await _dbContext.Sellers
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

        if (seller == null)
            return NotFound("Seller account not found");

        product.SellerId = seller.Id;

        var id = await _productService.CreateAsync(product, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
    
    [Authorize(Roles = "Seller")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        string? name,
        string? brand,
        string? description,
        string? sku,
        decimal? price,
        decimal? weight,
        decimal? parcelWeight,
        int? categoryId,
        int? countryId,
        string? ingredients,
        string? storageConditions,
        DateTime? expirationDate,
        string? article,
        int? stockQuantity,
        bool? trackInventory,
        List<string>? imageUrls,
        bool? isActive,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var seller = await _dbContext.Sellers
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

        if (seller == null)
            return NotFound("Seller account not found");

        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product == null)
            return NotFound();

        if (product.SellerId != seller.Id)
            return Forbid();

        var ok = await _productService.UpdateAsync(
            id,
            name,
            brand,
            description,
            sku,
            price,
            weight,
            parcelWeight,
            categoryId,
            countryId,
            ingredients,
            storageConditions,
            expirationDate,
            article,
            stockQuantity,
            trackInventory,
            imageUrls,
            isActive,
            ct);

        if (!ok)
            return NotFound();

        return Ok("Product updated");
    }

    [Authorize(Roles = "Seller")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var seller = await _dbContext.Sellers
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

        if (seller == null)
            return NotFound("Seller account not found");

        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product == null)
            return NotFound();

        if (product.SellerId != seller.Id)
            return Forbid();

        var ok = await _productService.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
  
    
    
    
}