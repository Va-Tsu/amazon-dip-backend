namespace AmazonClone.Domain.Entities;

public class RecentlyViewedProduct
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    
    public DateTime ViewedAt { get; set; } =  DateTime.UtcNow;
    
}