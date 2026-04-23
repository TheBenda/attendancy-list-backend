using Aspire.Hosting;
using Aspire.Hosting.Testing;

using TUnit.Core.Interfaces;

namespace EndToEndTests;

public class BaseDistributedHost : IAsyncInitializer, IAsyncDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    public DistributedApplication? _app;

    public string? DashboardUrl { get; private set; }
    public string? FrontendUrl { get; private set; }
    public string DashboardLoginToken { get; private set; } = "";

    public async Task InitializeAsync()
    {
        if (_app is not null) return;

        var ct = CancellationToken.None;

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ALB_AppHost>(ct);

        builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";

        var aspNetCoreUrls = builder.Configuration["ASPNETCORE_URLS"];
        var urls = aspNetCoreUrls is not null ? aspNetCoreUrls.Split(";") : [];

        DashboardUrl = urls.FirstOrDefault();

        //var viteUrl = builder.Configuration["VITE_APP_HTTP"];

        //FrontendUrl = viteUrl ?? throw new ArgumentNullException("VITE_APP_HTTP is not set");
        DashboardLoginToken = builder.Configuration["AppHost:BrowserToken"] ?? "";

        _app = await builder.BuildAsync(ct)
            .WaitAsync(DefaultTimeout, ct);

        await _app.StartAsync(ct);
    }


    public async ValueTask DisposeAsync()
    {
        if (_app != null) await _app.DisposeAsync();
    }
}