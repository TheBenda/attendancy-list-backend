using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Publishing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var vaultToken = builder.AddParameter("vault-token", secret: true);

// Kubernetes environment
var k8s = builder.AddKubernetesEnvironment("k8s");

//string acrDefaultUrl = "localhost:5555";
// determined by minikube ip
//string acrDefaultUrl = "192.168.64.3:5555";
string acrDefaultUrl = "localhost:5000";
string acrGhcrUrl = "ghcr.io";
var registryEndpoint = builder.AddParameter("registryEndpoint", acrDefaultUrl);
#pragma warning disable ASPIRECOMPUTE003
var acr = builder.AddContainerRegistry("acr", registryEndpoint);
k8s.WithContainerRegistry(acr);
#pragma warning restore ASPIRECOMPUTE003

var localtestGateway = k8s.AddGateway("localtest-gateway")
    .WithGatewayClass("traefik")
    .WithHostname("*.localtest.me")
    .WithGatewayAnnotation("cert-manager.io/cluster-issuer", "local-ca")
    .WithTls();

var vaultAddress = "http://127.0.0.1:8200";

var postgres = builder.AddPostgres("postgres")
    .WithPgWeb(containerName: "postgres-pgweb")
    .WithLifetime(ContainerLifetime.Persistent);

var postgresZitadel = builder.AddPostgres("postgres-zitadel")
    .WithPgWeb(containerName: "postgres-zitadel-pgweb")
    .WithLifetime(ContainerLifetime.Persistent);

var postgresZitadelDb = postgresZitadel.AddDatabase("postgres-zitadel-db");

var zitadel = builder.AddZitadel("zitadel")
    .WithDatabase(postgresZitadelDb)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithExternalHttpEndpoints();

localtestGateway.WithRoute("auth-route", "/", zitadel.GetEndpoint("http"));

var postgresdb = postgres.AddDatabase("postgresdb");

//var migrationService = builder.AddProject<Projects.ALB_MigrationService>("migration-service")
//    .WithReference(postgresdb)
//    .WaitFor(postgresdb);

var api = builder.AddProject<Projects.ALB_Api>("api")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    //.WaitForCompletion(migrationService)
    .WithExternalHttpEndpoints()
    //.WithEnvironment("Vault__Address", vaultAddress)
    .WithEnvironment("Vault__Token", vaultToken);

localtestGateway.WithRoute("api-route", "/", api.GetEndpoint("http"));

#pragma warning disable ASPIREJAVASCRIPT001
var viteApp = builder.AddViteApp("vite-app", "../../../attendance-list-frontend/", "ci")
    .WithPnpm()
    .WithHttpEndpoint(env: "PORT")
    .WithReference(api)
    .WithExternalHttpEndpoints()
    .PublishAsStaticWebsite();
#pragma warning restore ASPIREJAVASCRIPT001

localtestGateway.WithRoute("ui-route", "/", viteApp.GetEndpoint("http"));

var useMailpit = builder.Configuration.GetValue<bool>($"FeatureManagement:UseMailpit");

if (useMailpit)
{
#pragma warning disable ASPIRECOMPUTE003
    var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
        .WithVolume("mailpit-data", "/data")
        .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
        .WithEndpoint(port: 8025, targetPort: 8025, name: "http")
        .WithEnvironment("MP_DATABASE", "/data/mailpit.db")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithContainerRegistry(acr)
#pragma warning restore ASPIRECOMPUTE003
        .WithExternalHttpEndpoints();

    api.WithEnvironment("Mailpit__Host", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Host))
       .WithEnvironment("Mailpit__Port", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Port));
    
    localtestGateway.WithRoute("mailpit-route", "/", mailpit.GetEndpoint("http"));
}
#pragma warning disable ASPIRECOMPUTE003
var vault = builder.AddContainer("vault", "hashicorp/vault")
    .WithDockerfile("vault")
    .WithImageRegistry(acrDefaultUrl)
    .WithEndpoint(port: 8200, targetPort: 8200, name: "http", isExternal: true)
    .WithVolume("vault-data", "/vault/file")
    .WithContainerRuntimeArgs("--cap-add=IPC_LOCK")
    .WithEnvironment("VAULT_ADDR", vaultAddress)
    .WithContainerRegistry(acr)
#pragma warning restore ASPIRECOMPUTE003
    .WithLifetime(ContainerLifetime.Persistent);

localtestGateway.WithRoute("vault-route", "/", vault.GetEndpoint("http"));


api.WithReference(viteApp)
    .WaitFor(viteApp);


builder.Build().Run();