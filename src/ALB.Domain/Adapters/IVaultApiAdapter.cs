using ALB.Domain.Options;

namespace ALB.Domain.Adapters;

public interface IVaultApiAdapter
{
    Task<MailgunCredentials> GetMailgunCredentials();
}