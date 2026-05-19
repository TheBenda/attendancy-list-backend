namespace ALB.MailgunApi.Adapters;

public interface IMailgunApiAdapter
{
    Task SendInvitationEmailAsync(string to, string subject, string body);
}