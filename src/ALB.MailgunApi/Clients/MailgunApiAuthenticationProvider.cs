using System.Text;

using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace ALB.MailgunApi.Clients;

public class MailgunApiAuthenticationProvider: IAuthenticationProvider
{
    private readonly string _username = "api";
    private readonly string _password;

    public MailgunApiAuthenticationProvider(string password)
    {
        _password = password;
    }
    
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var rawCredentials = $"{_username}:{_password}";
        var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCredentials));

        request.Headers.Remove("Authorization");
        request.Headers.Add("Authorization", $"Basic {encodedCredentials}");

        return Task.CompletedTask;
    }
}