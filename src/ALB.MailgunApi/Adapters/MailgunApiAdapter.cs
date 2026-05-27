using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using ALB.Domain.Entities;
using ALB.Domain.Identity;
using ALB.Domain.Options;
using ALB.Domain.Values;
using ALB.MailgunApi.Clients;
using ALB.MailgunApi.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MimeKit;

using Mjml.Net;

namespace ALB.MailgunApi.Adapters;

public class MailgunApiAdapter: IMailgunApiAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CachedMailgunCredentialsProvider _credentialsProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailgunApiAdapter> _logger;

    public MailgunApiAdapter(IHttpClientFactory httpClientFactory, CachedMailgunCredentialsProvider credentialsProvider, IConfiguration configuration, ILogger<MailgunApiAdapter> logger)
    {
        _httpClientFactory = httpClientFactory;
        _credentialsProvider = credentialsProvider;
        _configuration = configuration;
        _logger = logger;
    }
    
    /// <summary>
    /// Sends an invitation email to the specified email address.
    /// The link is designed to macht the frontend url.
    /// </summary>
    /// <param name="receiver"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<string?> SendInvitationEmailAsync(InviteUser receiver, CancellationToken ct = default)
    {
        var credentials = await _credentialsProvider.GetAsync(ct);
        var httpClient = GetHttpClientAsync(credentials);
        
        if (string.IsNullOrWhiteSpace(receiver.Email))
            throw new ArgumentException("Email is required.", nameof(receiver));
        
        var message = new MimeMessage();

        var inviteLink = GenerateInvitationLink(receiver.Token);
        var html = await RenderInvitationEmailHtmlAsync(receiver, inviteLink, ct);
        var text = GenerateInvitationEmailText(receiver, inviteLink);

        message.From.Add(new MailboxAddress("Attendance List", $"postmaster@{credentials.Domain}"));
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

        await using var mimeStream = new MemoryStream();
        await message.WriteToAsync(mimeStream, ct);
        mimeStream.Position = 0;

        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(receiver.Email), "to");

        var messageContent = new StreamContent(mimeStream);
        messageContent.Headers.ContentType = new MediaTypeHeaderValue("message/rfc822");

        form.Add(messageContent, "message", "message.mime");
        
        var response = await httpClient.PostAsync($"v3/{credentials.Domain}/messages.mime", form, ct);
        
        response.EnsureSuccessStatusCode();

        var mailSendResponse = await response.Content.ReadFromJsonAsync<MailSendResponse>(ct);
        
        return mailSendResponse?.Id;
    }

    private HttpClient GetHttpClientAsync(MailgunCredentials credentials)
    {
        var user = "api";
        var password = credentials.ApiKey;
        
        var httpClient = _httpClientFactory.CreateClient(MailgunApiExtensions.MailgunClient);
        httpClient.BaseAddress = new Uri("https://api.mailgun.net");
        
        var rawCredentials = $"{user}:{password}";
        var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCredentials));

        httpClient.DefaultRequestHeaders.Remove("Authorization");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedCredentials}");
        
        return httpClient;
    }

    private string GenerateInvitationEmailText(InviteUser receiver, string inviteLink)
        => $"""
           Hello {receiver.FirstNames},
           
           you have been invited to create an account for the Attendance List system.
           
           Please open the following link to complete your registration:
           {receiver.Token}
           
           This invitation link expires in 3 hours.
           
           If you did not expect this invitation, you can ignore this email.
           """;

    private async Task<string> RenderInvitationEmailHtmlAsync(InviteUser receiver, string inviteLink, CancellationToken ct)
    {
        var mjmlRenderer = new MjmlRenderer();
        var mjml = await File.ReadAllTextAsync("Templates/InvitaionEmail.mjml", ct);
        
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

    private string GenerateInvitationLink(string token)
    {
        var frontendUrl = _configuration.GetValue<string>(ConfigNames.FrontendUrlKey) ??
                          throw new ArgumentException("Url could not be found in configuration.", nameof(ConfigNames.FrontendUrlKey));
        return $"{frontendUrl.TrimEnd('/')}/api/users/register-invited-user?token={token}";
    }

    public record MailSendResponse(string Id, string Message);
}