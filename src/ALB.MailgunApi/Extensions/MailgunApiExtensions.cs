using ALB.MailgunApi.Adapters;
using ALB.MailgunApi.Clients;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

using Polly;

namespace ALB.MailgunApi.Extensions;

public static class MailgunApiExtensions
{
    public static IServiceCollection AddMailgunApi(this IServiceCollection services)
    {
        services.AddKiotaHandlers();
        services.AddTransient<MailgunApiClientFactory>();
        services.AddHttpClient<MailgunApiClientFactory>((sp, client) =>
            {
                client.BaseAddress = new Uri("https://api.mailgun.net");
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

        services.AddTransient(sp => sp.GetRequiredService<MailgunApiClientFactory>().GetClient());

        services.AddTransient<IMailgunApiAdapter, MailgunApiAdapter>();
        
        return services;
    }
}