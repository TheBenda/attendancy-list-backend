namespace ALB.Domain.Options;

public class VaultOptions
{
    public const string SectionName = "Vault";
    
    public string Address { get; set; } = "";
    public string Token { get; set; } = "";
}