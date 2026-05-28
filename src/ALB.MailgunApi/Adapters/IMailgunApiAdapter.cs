using ALB.Domain.Entities;
using ALB.Domain.Identity;

namespace ALB.MailgunApi.Adapters;

public interface IMailgunApiAdapter
{
    Task<string?> SendInvitationEmailAsync(InviteUser receiver, CancellationToken cancellationToken = default);
}