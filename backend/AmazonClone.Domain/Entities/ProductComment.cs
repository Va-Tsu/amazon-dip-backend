namespace AmazonClone.Domain.Entities;

public class ProductComment
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;
    public string Text { get; set; } = "";
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;


}