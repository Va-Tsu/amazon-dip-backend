namespace AmazonClone.Domain.Entities;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public string Code { get; set; }
    public ICollection<Product>Products { get; set; }
}