using MailgunApi.Client;

namespace ALB.MailgunApi.Adapters;

public class MailgunApiAdapter(MailgunApiClient apiClient): IMailgunApiAdapter
{
    public Task SendInvitationEmailAsync(string to, string subject, string body)
    {
        
        throw new NotImplementedException();
    }
}