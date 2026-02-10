namespace AmazonClone.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? IdentityUserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}