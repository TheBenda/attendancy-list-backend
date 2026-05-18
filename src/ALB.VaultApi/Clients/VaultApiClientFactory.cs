using ALB.Domain.Options;

using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

using VaultApi.Client;

namespace ALB.VaultApi.Clients;

public class VaultApiClientFactory
{
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly HttpClient _httpClient;
    private readonly VaultOptions _options;

    public VaultApiClientFactory(HttpClient httpClient, IOptions<VaultOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _authenticationProvider = new VaultTokenAuthenticationProvider(_options.Token);
    }

    public VaultApiClient GetClient()
        => new VaultApiClient(new HttpClientRequestAdapter(_authenticationProvider, httpClient: _httpClient));
}