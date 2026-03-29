using System.Security.Claims;

using ALB.Api.Endpoints;
using ALB.Api.Exceptions;
using ALB.Application;
using ALB.Domain.Identity;
using ALB.Domain.Values;
using ALB.Infrastructure.Authentication;
using ALB.Infrastructure.Extensions;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

using Npgsql;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDataSource("postgresdb",
    configureDataSourceBuilder: sourceBuilder => sourceBuilder.UseNodaTime());

builder.AddServiceDefaults();

// This is needed to make OpenApi forwarding possible behind aspires reverse proxy.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var viteAppUrl = builder.Configuration
    .GetRequiredSection("VITE_APP_HTTP")
    .Value;

const string viteAppCorsPolicy = "ViteAppCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(viteAppCorsPolicy,
        pb =>
        {
            pb.WithOrigins(viteAppUrl ?? throw new InvalidOperationException("Url to VITE_APP_HTTP not set in environment variables."))
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});



builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method}{context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.TraceId);
    };
});

builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Version = "10.0";
        document.Info.Title = "Attendancy List Api .NET 10 API";
        document.Info.Description = "Rest API Definition for Attendancy List";
        document.Info.Contact = new OpenApiContact
        {
            Name = "André Benda",
            Email = "andre.benda@jambit.com"
        };
        document.Info.License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        };
        return Task.CompletedTask;
    });
});

//TODO: Implement new EmailSender and remove DummyEmailSender
builder.Services.AddTransient<IEmailSender<ApplicationUser>, DummyEmailSender>();

builder.Services.AddNodaTimeJsonConverters();

builder.Services
    .AddApplicationServices()
    .AddAuthAndIdentityCore(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(viteAppCorsPolicy);
app.UseExceptionHandler("/Error");
app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference("/api-reference", options =>
{
    options.WithTitle("ALB API")
        .WithTheme(ScalarTheme.Moon)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.MapEndpoints();
app.MapGet("/me", (ClaimsPrincipal claims) => Results.Ok(claims.Claims.ToDictionary(c => c.Type, c => c.Value)))
    .WithOpenApi()
    .RequireAuthorization(policy => policy.RequireRole(SystemRoles.Admin));

app.MapIdentityApiFilterable<ApplicationUser>(new IdentityApiEndpointRouteBuilderOptions
{
    ExcludeLoginPost = true,
    ExcludeRefreshPost = true,
    ExcludeConfirmEmailGet = false,
    ExcludeResendConfirmationEmailPost = false,
    ExcludeForgotPasswordPost = false,
    ExcludeResetPasswordPost = false,
    ExcludeRegisterPost = true,
    // Excluding ManageGroup hides 2FAPost, InfoGet and InfoPost
    ExcludeManageGroup = true,
    Exclude2FaPost = true,
    ExcludeInfoGet = true,
    ExcludeInfoPost = true
});

//app.UseTickerQ();

// TODO: add migrations when out of dev cycle
//using var serviceScope = app.Services.CreateScope();
//var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//await context.Database.EnsureCreatedAsync();

app.Run();