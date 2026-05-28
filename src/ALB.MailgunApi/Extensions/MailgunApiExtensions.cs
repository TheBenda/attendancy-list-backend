using ALB.Domain.Options;
using ALB.MailgunApi.Adapters;
using ALB.MailgunApi.Clients;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

using Polly;

namespace ALB.MailgunApi.Extensions;

public static class MailgunApiExtensions
{
    internal static string MailgunClient = "MailgunClient";
    public static IServiceCollection AddMailgunApi(this IServiceCollection services,
        FeatureFlagsOnStartup flagsOnStartup)
    {
        services.AddHttpClient(MailgunClient, (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri("https://api.mailgun.net");
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
        
        services.AddTransient<EmailBodyGenerator>();
        
        if (flagsOnStartup.UseMailpit)
        {
            services.AddScoped<IMailgunApiAdapter, MailpitSmtpAdapter>();
        }
        else
        {
            services.AddScoped<IMailgunApiAdapter, MailgunApiAdapter>();
        }
        
        services.AddScoped<CachedMailgunCredentialsProvider>();
        
        return services;
    }
}