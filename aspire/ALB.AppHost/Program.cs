using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var vaultToken = builder.AddParameter("vault-token", secret: true);

var vaultAddress = "http://127.0.0.1:8200";
var vault = builder.AddContainer("vault", "hashicorp/vault")
    .WithDockerfile("vault")
    .WithEndpoint(port: 8200, targetPort: 8200, name: "http")
    .WithVolume("vault-data", "/vault/file")
    .WithContainerRuntimeArgs("--cap-add=IPC_LOCK")
    .WithEnvironment("VAULT_ADDR", vaultAddress)
    .WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres")
    .WithPgWeb()
    .WithLifetime(ContainerLifetime.Persistent);

var postgresdb = postgres.AddDatabase("postgresdb");

var migrationService = builder.AddProject<Projects.ALB_MigrationService>("MigrationService")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var api = builder.AddProject<Projects.ALB_Api>("Api")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WaitFor(vault)
    .WaitForCompletion(migrationService)
    .WithEnvironment("Vault__Address", vaultAddress)
    .WithEnvironment("Vault__Token", vaultToken);

var viteApp = builder.AddViteApp("vite-app", "../../../attendance-list-frontend/")
    .WithPnpm()
    .WithReference(api);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Test"))
{
    var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
        .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
        .WithEndpoint(port: 8025, targetPort: 8025, name: "http");

    api.WithEnvironment("Mailpit__Host", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Host))
       .WithEnvironment("Mailpit__Port", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Port));
}

api.WithReference(viteApp)
    .WaitFor(viteApp);

/*

_ = builder.AddYarp("gateway") 
    .WithConfiguration(yarp =>
    {
        var cluster = new YarpCluster();
        yarp.AddCluster("api-cluster", destination: "http://alb-api");
        yarp.AddCluster("frontend-cluster", destination: "http://alb-frontend");
        
        yarp.AddRoute("/api/{**catch-all}", api);
        
        yarp.AddRoute("/{**catch-all}", viteApp);
    });
    */

builder.Build().Run();