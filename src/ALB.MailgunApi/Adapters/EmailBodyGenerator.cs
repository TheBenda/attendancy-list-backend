using ALB.Domain.Entities;
using ALB.Domain.Values;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Mjml.Net;

namespace ALB.MailgunApi.Adapters;

public class EmailBodyGenerator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailBodyGenerator> _logger;
    
    public EmailBodyGenerator(IConfiguration configuration, ILogger<EmailBodyGenerator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public string GenerateInvitationEmailText(InviteUser receiver, string inviteLink)
        => $"""
            Hello {receiver.FirstNames},

            you have been invited to create an account for the Attendance List system.

            Please open the following link to complete your registration:
            {inviteLink}

            This invitation link expires in 3 hours.

            If you did not expect this invitation, you can ignore this email.
            """;

    public async Task<string> RenderInvitationEmailHtmlAsync(InviteUser receiver, string inviteLink, CancellationToken ct)
    {
        var mjmlRenderer = new MjmlRenderer();
        var mjml = await ReadEmbeddedResourceAsync(
            "ALB.MailgunApi.Templates.InvitaionEmail.mjml",
            ct);
        
        mjml = mjml
            .Replace("{{firstName}}", receiver.FirstNames)
            .Replace("{{inviteLink}}", inviteLink)
            .Replace("{{expirationHours}}", "3");

        var options = new MjmlOptions
        {
            Beautify = false
        };

        var (html, errors) = await mjmlRenderer.RenderAsync(mjml, options, ct);

        if (errors.Count != 0)
        {
            _logger.LogWarning(errors.Count, "Error while rendering the email");
        }
        
        return html;
    }

    private static async Task<string> ReadEmbeddedResourceAsync(string resourceName, CancellationToken ct)
    {
        var assembly = typeof(EmailBodyGenerator).Assembly;

        await using var stream = assembly.GetManifestResourceStream(resourceName)
                                 ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(ct);
    }

    public string GenerateInvitationLink(Guid id)
    {
        var frontendUrl = _configuration.GetValue<string>(ConfigNames.FrontendUrlKeyHttps) ??
                          throw new ArgumentException("Url could not be found in configuration.", nameof(ConfigNames.FrontendUrlKeyHttps));
        return $"{frontendUrl.TrimEnd('/')}/register-invited/{id}";
    }
}