using ALB.Infrastructure.Persistence;
using ALB.MigrationService;

using Microsoft.EntityFrameworkCore;

using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDataSource("postgresdb",
    configureDataSourceBuilder: sourceBuilder => sourceBuilder.UseNodaTime());

builder.Services.AddHostedService<ApiDbInitializer>();

builder.AddServiceDefaults();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDbContextPool<ApplicationDbContext>((serviceProvider, options) =>
{
    var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
    options.UseNpgsql(dataSource, npgsqlOptions =>
        npgsqlOptions.UseNodaTime());
});

var app = builder.Build();

app.UseMigrationsEndPoint();
// TODO: add migrations when out of dev cycle
// using var serviceScope = app.Services.CreateScope();
// var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
// await context.Database.EnsureCreatedAsync();

app.Run();