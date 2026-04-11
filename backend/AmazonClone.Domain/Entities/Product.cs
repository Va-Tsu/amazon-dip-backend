using System.ComponentModel.DataAnnotations.Schema;
using AmazonClone.Domain.Enums;

namespace AmazonClone.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Brand { get; set; }
    public decimal Weight { get; set; }
    public decimal? ParcelWeight { get; set; }
    public string? Article { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; } 
    public int CountryId { get; set; }
    public Country? Country { get; set; } 
    public bool TrackInventory { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockTreshold { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; } = true;
    public string? StorageConditions { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Ingridients { get; set; }
    public string SKU { get; set; } = "";
    public decimal? OldPrice { get; set; }
    public bool HasDiscount { get; set; }
    public Guid? SellerId { get; set; } = default!;
    public Seller? Seller { get; set; } = default!;
    public ProductStatus Status { get; set; }
    
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    public ICollection<ProductComment> Comments { get; set; } = new List<ProductComment>();
    public ICollection<ProductParameter> Parameters { get; set; } = new List<ProductParameter>();
    public ICollection<OrderItem>OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem>CartItems { get; set; } = new List<CartItem>();

    


}