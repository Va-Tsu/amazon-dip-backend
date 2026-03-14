using Microsoft.AspNetCore.Identity;

namespace AmazonClone.Domain.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; }
    
    public ICollection<RecentlyViewedProduct> RecentlyViewedProducts { get; set; } = new List<RecentlyViewedProduct>();
    public ICollection<ProductComment> ProductComments { get; set; } = new List<ProductComment>();
}