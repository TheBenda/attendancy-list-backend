using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

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
    .WaitForCompletion(migrationService);

var viteApp = builder.AddViteApp("vite-app", "../../../attendance-list-frontend/")
    .WithReference(api);

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