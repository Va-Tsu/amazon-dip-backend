using AmazonClone.Domain.Enums;

namespace AmazonClone.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CartKey { get; set; }
    public OrderStatus Status{ get; set; }
    public decimal TotalPrice { get; set; }
    
    public List<OrderItem> Items { get; set; } 
}