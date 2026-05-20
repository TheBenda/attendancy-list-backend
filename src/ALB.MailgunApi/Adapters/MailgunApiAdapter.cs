using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using ALB.Domain.Identity;
using ALB.Domain.Options;
using ALB.MailgunApi.Clients;
using ALB.MailgunApi.Extensions;

using MimeKit;

namespace ALB.MailgunApi.Adapters;

public class MailgunApiAdapter: IMailgunApiAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CachedMailgunCredentialsProvider _credentialsProvider;

    public MailgunApiAdapter(IHttpClientFactory httpClientFactory, CachedMailgunCredentialsProvider credentialsProvider)
    {
        _httpClientFactory = httpClientFactory;
        _credentialsProvider = credentialsProvider;
    }
    
    /// <summary>
    /// Sends an email to the specified user.
    /// </summary>
    /// <param name="receiver"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<string?> SendInvitationEmailAsync(ApplicationUser receiver, string subject, string body, CancellationToken ct = default)
    {
        var credentials = await _credentialsProvider.GetAsync(ct);
        var httpClient = GetHttpClientAsync(credentials);
        
        if (string.IsNullOrWhiteSpace(receiver.Email))
            throw new ArgumentException("Email is required.", nameof(receiver));
        
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Attendance List", $"postmaster@{credentials.Domain}"));
        message.To.Add(new MailboxAddress(
            $"{receiver.FirstName} {receiver.LastName}".Trim(),
            receiver.Email));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = body,
            HtmlBody = $"<p>{System.Net.WebUtility.HtmlEncode(body)}</p>"
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

    public record MailSendResponse(string Id, string Message);
}