using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Publishing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Microsoft.Extensions.DependencyInjection;

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

var zitadelMasterKey = builder.AddParameter("zitadel-masterKey", secret: true);
var zitadelAdminPassword = builder.AddParameter("zitadel-password", secret: true);
var zitadelLoginPat = builder.AddParameter("zitadel-login-pat", secret: true);

var zitadel = builder.AddContainer("zitadel", "ghcr.io/zitadel/zitadel", "v4.15.0")
    .WithArgs("start-from-init", "--masterkeyFromEnv", "--tlsMode", "external")
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithExternalHttpEndpoints()
    // TLS is terminated by Traefik gateway, Zitadel runs plain HTTP
    .WithEnvironment("ZITADEL_TLS_ENABLED", "false")
    .WithEnvironment("ZITADEL_EXTERNALSECURE", "true")
    .WithEnvironment("ZITADEL_EXTERNALDOMAIN", "auth.localtest.me")
    .WithEnvironment("ZITADEL_EXTERNALPORT", "443")
    .WithEnvironment("ZITADEL_MASTERKEY", zitadelMasterKey)
    // Database
    .WithEnvironment("ZITADEL_DATABASE_POSTGRES_HOST", postgresZitadel.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("ZITADEL_DATABASE_POSTGRES_PORT", postgresZitadel.Resource.PrimaryEndpoint.Property(EndpointProperty.Port))
    .WithEnvironment("ZITADEL_DATABASE_POSTGRES_DATABASE", postgresZitadelDb.Resource.DatabaseName)
    .WithEnvironment("ZITADEL_DATABASE_POSTGRES_ADMIN_USERNAME", postgresZitadel.Resource.UserNameReference)
    .WithEnvironment("ZITADEL_DATABASE_POSTGRES_ADMIN_PASSWORD", postgresZitadel.Resource.PasswordParameter)
    .WithEnvironment("ZITADEL_DATABASE_POSTGRES_USER_USERNAME", postgresZitadel.Resource.UserNameReference)
    .WithEnvironment("ZITADEL_DATABASE_POSTGRES_USER_PASSWORD", postgresZitadel.Resource.PasswordParameter)
    // Admin user
    .WithEnvironment("ZITADEL_FIRSTINSTANCE_ORG_HUMAN_USERNAME", "admin")
    .WithEnvironment("ZITADEL_FIRSTINSTANCE_ORG_HUMAN_PASSWORD", zitadelAdminPassword)
    .WithEnvironment("ZITADEL_FIRSTINSTANCE_ORG_HUMAN_PASSWORDCHANGEREQUIRED", "false")
    // Login V2
    .WithEnvironment("ZITADEL_DEFAULTINSTANCE_FEATURES_LOGINV2_REQUIRED", "true")
    .WithEnvironment("ZITADEL_DEFAULTINSTANCE_FEATURES_LOGINV2_BASEURI", "https://auth.localtest.me/ui/v2/login/")
    .WithEnvironment("ZITADEL_OIDC_DEFAULTLOGINURLV2", "https://auth.localtest.me/ui/v2/login/login?authRequest=")
    .WithEnvironment("ZITADEL_OIDC_DEFAULTLOGOUTURLV2", "https://auth.localtest.me/ui/v2/login/logout?post_logout_redirect=")
    .WithEnvironment("ZITADEL_SAML_DEFAULTLOGINURLV2", "https://auth.localtest.me/ui/v2/login/login?samlRequest=")
    // PAT bootstrapping for login container
    .WithEnvironment("ZITADEL_FIRSTINSTANCE_LOGINCLIENTPATPATH", "/zitadel/bootstrap/login-client.pat")
    .WithEnvironment("ZITADEL_FIRSTINSTANCE_ORG_LOGINCLIENT_MACHINE_USERNAME", "login-client")
    .WithEnvironment("ZITADEL_FIRSTINSTANCE_ORG_LOGINCLIENT_MACHINE_NAME", "Automatically Initialized IAM_LOGIN_CLIENT")
    .WithEnvironment("ZITADEL_FIRSTINSTANCE_ORG_LOGINCLIENT_PAT_EXPIRATIONDATE", "2030-01-01T00:00:00Z")
    .WithVolume("zitadel-bootstrap", "/zitadel/bootstrap")
    .WithReference(postgresZitadelDb)
    .WaitFor(postgresZitadelDb);

var zitadelLogin = builder.AddContainer("zitadel-login", "ghcr.io/zitadel/zitadel-login", "v4.15.0")
    .WithHttpEndpoint(targetPort: 3000, name: "http")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ZITADEL_API_URL", "http://zitadel-service:8080")
    .WithEnvironment("NEXT_PUBLIC_BASE_PATH", "/ui/v2/login")
    .WithEnvironment("ZITADEL_SERVICE_USER_TOKEN", zitadelLoginPat)
    .WithEnvironment("CUSTOM_REQUEST_HEADERS", "Host:auth.localtest.me,X-Forwarded-Proto:https")
    .WaitFor(zitadel);

// Login V2 UI (longer prefix matches first in Gateway API)
localtestGateway.WithRoute("auth.localtest.me", "/ui/v2/login", zitadelLogin.GetEndpoint("http"));
// Everything else (API, console, OIDC endpoints, etc.)
localtestGateway.WithRoute("auth.localtest.me", "/", zitadel.GetEndpoint("http"));

var postgresdb = postgres.AddDatabase("postgresdb");

//var migrationService = builder.AddProject<Projects.ALB_MigrationService>("migration-service")
//    .WithReference(postgresdb)
//    .WaitFor(postgresdb);

#pragma warning disable ASPIREPIPELINES003
var api = builder.AddProject<Projects.ALB_Api>("api")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    //.WaitForCompletion(migrationService)
    .WithExternalHttpEndpoints()
    //.WithEnvironment("Vault__Address", vaultAddress)
    .WithEnvironment("Vault__Token", vaultToken)
    .PublishAsDockerFile(cb =>
    {
        // Context must be repo root so the Dockerfile can COPY sibling projects
        cb.WithDockerfile(contextPath: "../..", dockerfilePath: "src/ALB.Api/Dockerfile");
    })
    .WithContainerBuildOptions(ctx =>
    {
        ctx.TargetPlatform = ContainerTargetPlatform.LinuxArm64;
    });
#pragma warning restore ASPIREPIPELINES003

localtestGateway.WithRoute("api.localtest.me", "/", api.GetEndpoint("http"));

#pragma warning disable ASPIREJAVASCRIPT001
#pragma warning disable ASPIREPIPELINES003
var viteApp = builder.AddViteApp("vite-app", "../../../attendance-list-frontend/", "ci")
    .WithPnpm()
    .WithHttpEndpoint(env: "PORT")
    .WithReference(api)
    .WithExternalHttpEndpoints()
    .PublishAsStaticWebsite()
    .WithContainerBuildOptions(ctx =>
    {
        ctx.TargetPlatform = ContainerTargetPlatform.LinuxArm64;
    });
#pragma warning restore ASPIREPIPELINES003
#pragma warning restore ASPIREJAVASCRIPT001

localtestGateway.WithRoute("ui.localtest.me", "/", viteApp.GetEndpoint("http"));

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
    
    localtestGateway.WithRoute("mailpit.localtest.me", "/", mailpit.GetEndpoint("http"));
}
#pragma warning disable ASPIRECOMPUTE003
#pragma warning disable ASPIREPIPELINES003
var vault = builder.AddContainer("vault", "hashicorp/vault")
    .WithDockerfile("vault")
    .WithImageRegistry(acrDefaultUrl)
    .WithEndpoint(port: 8200, targetPort: 8200, name: "http", isExternal: true)
    .WithVolume("vault-data", "/vault/file")
    .WithContainerRuntimeArgs("--cap-add=IPC_LOCK")
    .WithEnvironment("VAULT_ADDR", vaultAddress)
    .WithContainerRegistry(acr)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithContainerBuildOptions(ctx =>
    {
        ctx.TargetPlatform = ContainerTargetPlatform.LinuxArm64;
    });
#pragma warning restore ASPIREPIPELINES003
#pragma warning restore ASPIRECOMPUTE003

localtestGateway.WithRoute("vault.localtest.me", "/", vault.GetEndpoint("http"));


api.WithReference(viteApp)
    .WaitFor(viteApp);

builder.Build().Run();


