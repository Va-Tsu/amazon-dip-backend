using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonClone.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Weight { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    
    [NotMapped]
    public decimal CurrentPrice { get; set; }
    
    [NotMapped]
    public decimal? OldPrice { get; set; }
    
    [NotMapped]
    public bool HasDiscount { get; set; }


}