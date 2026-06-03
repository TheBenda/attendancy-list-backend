using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using Microsoft.Extensions.Configuration;
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

var migrationService = builder.AddProject<Projects.ALB_MigrationService>("migration-service")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var api = builder.AddProject<Projects.ALB_Api>("api")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WaitFor(vault)
    .WaitForCompletion(migrationService)
    .WithEnvironment("Vault__Address", vaultAddress)
    .WithEnvironment("Vault__Token", vaultToken);

#pragma warning disable ASPIRECERTIFICATES001
var viteApp = builder.AddViteApp("vite-app", "../../../attendance-list-frontend/")
    .WithPnpm()
    .WithHttpsEndpoint(env: "PORT")
    .WithHttpsDeveloperCertificate()
#pragma warning restore ASPIRECERTIFICATES001
    .WithReference(api);

var useMailpit = builder.Configuration.GetValue<bool>($"FeatureManagement:UseMailpit");

if (useMailpit)
{
    var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
        .WithVolume("mailpit-data", "/data")
        .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
        .WithEndpoint(port: 8025, targetPort: 8025, name: "http")
        .WithEnvironment("MP_DATABASE", "/data/mailpit.db")
        .WithLifetime(ContainerLifetime.Persistent);

    api.WithEnvironment("Mailpit__Host", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Host))
       .WithEnvironment("Mailpit__Port", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Port));
}



//var gateway = builder.AddProject<Projects.ALB_Gateway>("gateway")
//    .WithReference(api)
//    .WaitFor(api)
//    .WithReference(viteApp)
//    .WaitFor(viteApp);
    //.WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Path", "../../alb-frontend.pem")
    //.WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__KeyPath", "../../alb-frontend-key.pem");
//#pragma warning disable ASPIRECERTIFICATES001
//var gateway = builder.AddYarp("gateway")
//    .WithHttpsEndpoint()
//    .WithHttpsDeveloperCertificate()
//#pragma warning restore ASPIRECERTIFICATES001
//    .WithConfiguration(yarp =>
//    {
//        yarp.AddRoute("/api/{**catch-all}", api);
//        
//        yarp.AddRoute("/{**catch-all}", viteApp);
//    });

api.WithReference(viteApp)
    //.WithReference(gateway)
    .WaitFor(viteApp);

//viteApp.WithReference(gateway);



builder.Build().Run();