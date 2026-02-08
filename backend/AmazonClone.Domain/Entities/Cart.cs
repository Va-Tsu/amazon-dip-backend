using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonClone.Domain.Entities;

public class Cart
{
    public Guid Id { get; set;}
    public string? IdentityUserId { get;set ;}
    public string? SessionId { get;set;}
    public List<CartItem> Items { get; set; } = new();

    [NotMapped] public decimal TotalSum => Items.Sum(i => i.TotalPrice);
}