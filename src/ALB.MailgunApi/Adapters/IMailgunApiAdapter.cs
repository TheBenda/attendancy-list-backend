using ALB.Domain.Identity;

namespace ALB.MailgunApi.Adapters;

public interface IMailgunApiAdapter
{
    Task<string?> SendInvitationEmailAsync(ApplicationUser receiver, string subject, string body, CancellationToken cancellationToken = default);
}