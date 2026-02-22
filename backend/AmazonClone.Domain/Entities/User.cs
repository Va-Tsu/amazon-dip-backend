using Microsoft.AspNetCore.Identity;

namespace AmazonClone.Domain.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; }
}