using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace ALB.VaultApi.Clients;

public class VaultTokenAuthenticationProvider: IAuthenticationProvider
{
    private readonly string _token;
    
    public VaultTokenAuthenticationProvider(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Vault token must not be empty.", nameof(token));

        _token = token;
    }
    
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        request.Headers.Add("X-Vault-Token", _token);
        return Task.CompletedTask;
    }
}