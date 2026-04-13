using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Domain.Enums;
using AmazonClone.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AmazonClone.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SellerController:ControllerBase
{
    readonly ApplicationDbContext _dbContext;
    readonly UserManager<User> _userManager;
    readonly SignInManager<User> _signInManager;
    readonly IProductService _productService;
    readonly IConfiguration _config;

    public SellerController(ApplicationDbContext dbContext,
        UserManager<User> userManager, SignInManager<User> signInManager,
        IProductService productService, IConfiguration config)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _productService = productService;
        _config = config;
       

    }
    
    [Authorize]
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
            if (password != confirmedPassword)
            {
                return BadRequest("Passwords do not match");
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
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Email and password are required");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Unauthorized("Invalid email");
        }
        
        if (!string.IsNullOrWhiteSpace(fullName) &&
            !string.Equals(user.FullName, fullName, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Entered full name does not match current user");

        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!passwordCheck.Succeeded)
            return Unauthorized("Invalid password");

        var seller = await _dbContext.Sellers
            .AsNoTracking()
            .Include(s=>s.User)
            .FirstOrDefaultAsync(s => s.UserId == user.Id, ct);

        if (seller == null)
        {
            return BadRequest("Seller account not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Seller"))
        {
            return BadRequest("User does not have Seller role");
        }
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        
        var jwt = _config.GetSection("Jwt");
        var getKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(getKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpireMinutes"])),
            signingCredentials: creds);
        
        return Ok(new
        {
            message = "Seller access granted",
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo,
            roles,
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
    public async Task<IActionResult> Create([FromForm] string name,
        [FromForm] string description,
        [FromForm] string brand,
        [FromForm] decimal weight,
        [FromForm] decimal? parcelWeight,
        [FromForm] string? article,
        [FromForm] decimal price,
        [FromForm] int categoryId,
        [FromForm] int countryId,
        [FromForm] bool trackInventory,
        [FromForm] bool isActive,
        [FromForm] bool isPublished,
        [FromForm] string? storageConditions,
        [FromForm] DateTime? expirationDate,
        [FromForm] string? ingridients,
        [FromForm] string sku,
        [FromForm] ProductStatus status,
        [FromForm] List<IFormFile>? images,
        [FromForm] decimal? costOfGoods,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var seller = await _dbContext.Sellers
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

        if (seller == null)
            return NotFound("Seller account not found");
        
        var imageUrls = new List<string>();

        if (images != null && images.Count > 0)
        {
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "products");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var image in images)
            {
                if (image == null || image.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only .jpg, .jpeg, .png and .webp files are allowed");
                }

                if (!image.ContentType.StartsWith("image/"))
                {
                    return BadRequest("Uploaded file must be an image");
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream, ct);

                imageUrls.Add($"/images/products/{fileName}");
            }
        }
        
        var product = new Product
        {
            Name = name,
            Description = description,
            Brand = brand,
            Weight = weight,
            ParcelWeight = parcelWeight,
            Article = article,
            Price = price,
            CategoryId = categoryId,
            CountryId = countryId,
            TrackInventory = trackInventory,
            CreatedAt = DateTime.UtcNow,
            IsActive = isActive,
            IsPublished = isPublished,
            StorageConditions = storageConditions,
            ExpirationDate = expirationDate,
            Ingridients = ingridients,
            SKU = sku,
            HasDiscount = false,
            SellerId = seller.Id,
            Status = status
        };
        
        var id = await _productService.CreateAsync(product, imageUrls, ct);
        
        decimal? profit = null;
        if (costOfGoods.HasValue && costOfGoods.Value < price)
        {
            profit = price - costOfGoods.Value;
        }
        decimal? margin = null;
        if (profit.HasValue && price>0)
        {
            margin = (profit.Value / price) * 100;
        }
        
        return CreatedAtAction(nameof(GetById), new { id }, new { id, profit, margin });
    }
    
    [Authorize(Roles = "Seller")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromForm]string name,
        [FromForm] string description,
        [FromForm] string brand,
        [FromForm] decimal weight,
        [FromForm] decimal? parcelWeight,
        [FromForm] string? article,
        [FromForm] decimal price,
        [FromForm] int categoryId,
        [FromForm] int countryId,
        [FromForm] bool trackInventory,
        [FromForm] bool isActive,
        [FromForm] bool isPublished,
        [FromForm] string? storageConditions,
        [FromForm] DateTime? expirationDate,
        [FromForm] string? ingridients,
        [FromForm] bool hasDiscount,
        [FromForm] string sku,
        [FromForm] ProductStatus status,
        [FromForm] List<IFormFile>? images,
        [FromForm] decimal? costOfGoods,
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
        
        var savedImageUrls = new List<string>();

        if (images != null && images.Count > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "products");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var image in images)
            {
                if (image == null || image.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only .jpg, .jpeg, .png and .webp files are allowed");
                }

                if (!image.ContentType.StartsWith("image/"))
                {
                    return BadRequest("The uploaded file must be an image");
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream, ct);

                savedImageUrls.Add($"/images/products/{fileName}");
            }
        }

        var ok = await _productService.UpdateAsync(
            id,
            name,
            brand,
            description,
            sku,
            weight,
            parcelWeight,
            categoryId,
            countryId,
            ingridients,
            storageConditions,
            expirationDate,
            article,
            trackInventory,
            price,
            hasDiscount,
            savedImageUrls,
            isActive,
            isPublished,
            seller.Id,
            status,
            ct);


        if (!ok)
            return NotFound();
        
        decimal? profit = null;
        if (costOfGoods.HasValue && costOfGoods.Value < price)
        {
            profit = price - costOfGoods.Value;
        }
        decimal? margin = null;
        if (profit.HasValue && price>0)
        {
            margin = (profit.Value / price) * 100;
        }

        return Ok(
            new
            {
                message = "Product updated",
                calcullation = new
                {
                    profit,
                    margin
                }
            });
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