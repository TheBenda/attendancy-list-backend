using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

using ALB.Api;
using ALB.Application.UseCases.Auths;
using ALB.Domain.Identity;
using ALB.Domain.Values;
using ALB.Infrastructure.Persistence;
using ALB.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Testcontainers.PostgreSql;

using TUnit.Core.Interfaces;

namespace ApiIntegrationTests;

public class BaseIntegrationTest : IAsyncInitializer, IAsyncDisposable
{
    public const string AdminEmail = "admin@attendance-list-backend.de";
    public const string AdminPassword = "SoSuperSecureP4a55w0rd!";
    public const string ParentEmail = "parent@attendance-list-backend.de";
    public const string ParentPassword = "ParentP4a55w0rd!";
    public static string AdminToken = string.Empty;
    public static string ParentToken = string.Empty;

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("postgres")
        .Build();

    private WebApplicationFactory<IApiAssemblyMarker> _webApplicationFactory = null!;

    public async ValueTask DisposeAsync()
    {
        await _webApplicationFactory.DisposeAsync();
        await _postgreSqlContainer.StopAsync();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        _webApplicationFactory = new TestWebApplicationFactory(_postgreSqlContainer.GetConnectionString());

        _ = _webApplicationFactory.Server;

        using var serviceScope = _webApplicationFactory.Services.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // await context.Database.MigrateAsync();
        await context.Database.EnsureCreatedAsync();

        var seeder = new PowerUserSeederService(serviceScope.ServiceProvider);
        await seeder.StartAsync(CancellationToken.None);

        var parent = new ApplicationUser
        {
            Email = ParentEmail,
            UserName = ParentEmail,
            FirstName = "Hans",
            LastName = "Hausmann"
        };

        var identityRes = await this.UserManager.CreateAsync(parent, ParentPassword);

        if (identityRes.Succeeded)
        {
            await this.UserManager.AddToRoleAsync(parent, SystemRoles.Parent);
        }

        AdminToken = await TokenProvider.Create(await this.UserManager.FindByEmailAsync(AdminEmail) ?? throw new InvalidOperationException(), new List<string>{SystemRoles.Admin});
        ParentToken = await TokenProvider.Create(await this.UserManager.FindByEmailAsync(ParentEmail) ?? throw new InvalidOperationException(), new List<string>{SystemRoles.Parent});
    }

    private HttpClient GetHttpClient(string token)
    {
        var client = _webApplicationFactory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Api-Key",
            token
        );

        return client;
    }

    public IServiceScope GetScope()
    {
        return _webApplicationFactory.Services.CreateScope();
    }

    public UserManager<ApplicationUser> UserManager => _webApplicationFactory.Services.GetRequiredService<UserManager<ApplicationUser>>();
    public TokenProvider TokenProvider => _webApplicationFactory.Services.GetRequiredService<TokenProvider>();

    public HttpClient GetAdminClient()
    {
        var client = _webApplicationFactory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Api-Key",
            SystemRoles.Admin
        );

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
        return client;
    }

    public HttpClient GetCoAdminClient()
    {
        return GetHttpClient(SystemRoles.CoAdmin);
    }

    public HttpClient GetTeamClient()
    {
        return GetHttpClient(SystemRoles.Team);
    }

    public HttpClient GetParentClient()
    {
        var client = _webApplicationFactory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Api-Key",
            SystemRoles.Parent
        );

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ParentToken);
        return client;
    }
}

internal class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = Request.Headers["X-Api-Key"].ToString();

        var claims = apiKey switch
        {
            SystemRoles.Admin => new[]
            {
                new Claim(ClaimTypes.Name, "AdminUser"),
                new Claim(ClaimTypes.Role, "Admin")
            },
            SystemRoles.Parent => new[]
            {
                new Claim(ClaimTypes.Name, "ParentUser"),
                new Claim(ClaimTypes.Role, "Parent")
            },
            _ => throw new InvalidOperationException("Unknown API Key")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

file sealed class TestWebApplicationFactory(string connectionString)
    : WebApplicationFactory<IApiAssemblyMarker>
{

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureHostConfiguration(configBuilder =>
        {
            configBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", false);
        });
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            };
            configBuilder.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureServices(services =>
        {

            var dbContextDescriptor =
                services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            var powerUserSeederDescriptor =
                services.SingleOrDefault(s => s.ServiceType == typeof(IHostedService) &&
                                              s.ImplementationType == typeof(ALB.Infrastructure.Services.PowerUserSeederService));

            if (powerUserSeederDescriptor is not null)
                services.Remove(powerUserSeederDescriptor);

            services.AddDbContextPool<ApplicationDbContext>((container, options) =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNodaTime());
            });

            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            services.AddAuthorizationBuilder()
                .AddPolicy(SystemRoles.AdminPolicy, x => x.RequireRole(SystemRoles.Admin))
                .AddPolicy(SystemRoles.CoAdminPolicy, x => x.RequireRole(SystemRoles.CoAdmin))
                .AddPolicy(SystemRoles.TeamPolicy, x => x.RequireRole(SystemRoles.Team))
                .AddPolicy(SystemRoles.ParentPolicy, x => x.RequireRole(SystemRoles.Parent));
        });
    }
}