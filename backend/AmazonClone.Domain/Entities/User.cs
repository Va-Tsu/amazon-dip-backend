namespace AmazonClone.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? IdentityUserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateOnly Birthday { get; set; }
}