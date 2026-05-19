using ALB.Domain.Adapters;
using ALB.Domain.Options;

using MailgunApi.Client;

using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace ALB.MailgunApi.Clients;

public class MailgunApiClientFactory
{
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly HttpClient _httpClient;
    private readonly MailgunCredentials _mailgunCredentials;

    public MailgunApiClientFactory(HttpClient httpClient, IVaultApiAdapter vaultApiAdapter)
    {
        var mailgunCredentials = vaultApiAdapter.GetMailgunCredentials().Result;
        _mailgunCredentials = mailgunCredentials;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_mailgunCredentials.BaseUrl);
        _authenticationProvider = new MailgunApiAuthenticationProvider(_mailgunCredentials.ApiKey);
    }

    public MailgunApiClient GetClient()
        => new MailgunApiClient(new HttpClientRequestAdapter(_authenticationProvider, httpClient: _httpClient));
}