namespace ALB.Domain.Entities;

public class InviteUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string FirstNames { get; set; }
    public required string LastNames { get; set; }
}