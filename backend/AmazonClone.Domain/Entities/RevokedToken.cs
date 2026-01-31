namespace AmazonClone.Domain.Entities;

public class RevokedToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
}