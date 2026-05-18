using ALB.Domain.Options;
using ALB.VaultApi.Adapters;
using ALB.VaultApi.Clients;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

using Polly;

namespace ALB.VaultApi.Extensions;

public static class VaultApiExtensions
{
    public static IServiceCollection AddVaultApiAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        var smartFaceConfigurationOption = configuration.GetSection(VaultOptions.SectionName)
            .Get<VaultOptions>();
        
        services.AddKiotaHandlers();
        services.AddTransient<VaultApiClientFactory>();
        services.AddHttpClient<VaultApiClientFactory>((sp, client) =>
            {
                client.BaseAddress = new Uri(smartFaceConfigurationOption!.Address);
            })
            .AttachKiotaHandlers()
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
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

        services.AddTransient(sp => sp.GetRequiredService<VaultApiClientFactory>().GetClient());

        services.AddTransient<IVaultApiAdapter, VaultApiAdapter>();

        return services;
    }
}