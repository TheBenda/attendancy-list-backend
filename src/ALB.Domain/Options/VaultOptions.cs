namespace ALB.Domain.Options;

public class VaultOptions
{
    public const string SectionName = "Vault";
    
    public required string Address { get; set; }
    public required string Token { get; set; }
}