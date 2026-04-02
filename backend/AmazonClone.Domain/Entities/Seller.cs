using Microsoft.AspNetCore.Identity;

namespace AmazonClone.Domain.Entities;

public class Seller
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string StoreName { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    //public int CountryId { get; set; }
    //public Country Country { get; set; }
    public string Country { get; set; }

    public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal PandingBalance { get; set; }
    public bool IsActive { get; set; } =  true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();

}