namespace ALB.VaultApi.Adapters;

public interface IVaultApiAdapter
{
    Task<GraphApiCredentials> GetToken();
    Task<MailgunCredentials> GetMailgunCredentials();
}