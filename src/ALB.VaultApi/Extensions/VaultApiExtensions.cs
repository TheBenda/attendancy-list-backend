using ALB.Domain.Adapters;
using ALB.Domain.Options;
using ALB.VaultApi.Adapters;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Polly;

namespace ALB.VaultApi.Extensions;

public static class VaultApiExtensions
{
    internal static string ApiVaultClient = "VaultClient";
    public static IServiceCollection AddVaultApiAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.BuildServiceProvider();
        services.AddHttpClient(ApiVaultClient, (serviceProvider, client) =>
            {
                var vaultOptions = serviceProvider
                    .GetRequiredService<IOptions<VaultOptions>>().Value;
            
                client.DefaultRequestHeaders.Add("X-Vault-Token", vaultOptions.Token);
            
                client.BaseAddress = new Uri(vaultOptions.Address);
            }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddResilienceHandler("SmartFace-pipeline", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Constant,
                    Delay = TimeSpan.FromMilliseconds(100)
                });
            });

        services.AddScoped<IVaultApiAdapter, VaultApiAdapter>();

        return services;
    }
}