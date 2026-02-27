using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonClone.Domain.Entities;

public class Cart
{
    public Guid Id { get; set;}
    public string? UserId { get;set ;}
    public string? CartKey { get;set;}
    public decimal TotalPrice { get;set;}
    public List<CartItem> Items { get; set; } = new();
    
}