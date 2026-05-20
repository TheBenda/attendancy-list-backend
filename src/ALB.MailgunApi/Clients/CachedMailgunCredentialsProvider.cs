using ALB.Domain.Adapters;
using ALB.Domain.Options;

using Microsoft.Extensions.Caching.Memory;

namespace ALB.MailgunApi.Clients;

public class CachedMailgunCredentialsProvider
{
    private const string CacheKey = "mailgun-credentials";
    private readonly IMemoryCache _cache;
    private readonly IVaultApiAdapter _vaultApiAdapter;

    public CachedMailgunCredentialsProvider(
        IMemoryCache cache,
        IVaultApiAdapter vaultApiAdapter)
    {
        _cache = cache;
        _vaultApiAdapter = vaultApiAdapter;
    }
    
    public async Task<MailgunCredentials> GetAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _vaultApiAdapter.GetMailgunCredentials("v1", ct);
        }) ?? throw new InvalidOperationException("Mailgun credentials could not be loaded.");
    }
}