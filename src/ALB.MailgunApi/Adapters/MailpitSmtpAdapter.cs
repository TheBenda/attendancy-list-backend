using ALB.Domain.Entities;
using ALB.Domain.Identity;
using ALB.Domain.Options;
using ALB.Domain.Values;

using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Mjml.Net;

namespace ALB.MailgunApi.Adapters;

public class MailpitSmtpAdapter : IMailgunApiAdapter
{
    private readonly MailpitOptions _options;
    private readonly ILogger<MailpitSmtpAdapter> _logger;
    private readonly EmailBodyGenerator _emailBodyGenerator;

    public MailpitSmtpAdapter(IOptions<MailpitOptions> options, ILogger<MailpitSmtpAdapter> logger, EmailBodyGenerator emailBodyGenerator)
    {
        _options = options.Value;
        _logger = logger;
        _emailBodyGenerator = emailBodyGenerator;
    }

    public async Task<string?> SendInvitationEmailAsync(InviteUser receiver, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(receiver.Email))
            throw new ArgumentException("Email is required.", nameof(receiver));

        var message = new MimeMessage();
        var inviteLink = _emailBodyGenerator.GenerateInvitationLink(receiver.Token);
        var html = await _emailBodyGenerator.RenderInvitationEmailHtmlAsync(receiver, inviteLink, ct);
        var text = _emailBodyGenerator.GenerateInvitationEmailText(receiver, inviteLink);

        message.From.Add(new MailboxAddress("Attendance List", "postmaster@localhost"));
        message.To.Add(new MailboxAddress(
            $"{receiver.FirstNames} {receiver.LastNames}".Trim(),
            receiver.Email));
        message.Subject = "You have been invited to join the Attendance List";

        var builder = new BodyBuilder
        {
            TextBody = text,
            HtmlBody = html
        };

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, MailKit.Security.SecureSocketOptions.None, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        return Guid.NewGuid().ToString();
    }
}