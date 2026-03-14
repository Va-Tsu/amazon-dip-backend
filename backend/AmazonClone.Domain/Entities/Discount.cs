namespace AmazonClone.Domain.Entities;

public class Discount
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    
    public decimal? DiscountPrice { get; set; }
    public int? DiscountPersentage { get; set; }
    
    public DateTime? StartAt{get; set;}
    public DateTime? EndAt{get; set;}

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}