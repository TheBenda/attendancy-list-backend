# OpenCode / AI Agent Instructions

This repository is a .NET 10 Minimal API backend using EF Core, PostgreSQL, and .NET Aspire.

## 🛠️ Core Commands
- **Solution** The project uses the new .slnx format
- **Add dependencies** Always add dependencies using the dotnet cli to ensure no conflicts with centralized package managers
- **Run API:** `dotnet run --project src/ALB.Api/ALB.Api.csproj`
- **Run Integration Tests:** `dotnet test tests/ApiIntegrationTests/ApiIntegrationTests/ApiIntegrationTests.csproj -e ASPNETCORE_ENVIRONMENT=Test`
  - *Note:* Docker MUST be running locally (uses `Testcontainers.PostgreSql`).
- **Run single test:** `dotnet test tests/ApiIntegrationTests/ApiIntegrationTests/ApiIntegrationTests.csproj --filter "TestName=<YourTestName>" -e ASPNETCORE_ENVIRONMENT=Test`
- **Add Migration:** `dotnet ef migrations add <Name> --project src/ALB.Infrastructure/ALB.Infrastructure.csproj --startup-project src/ALB.Api/ALB.Api.csproj`
- **Update Database:** `dotnet ef database update --project src/ALB.Infrastructure/ALB.Infrastructure.csproj --startup-project src/ALB.Api/ALB.Api.csproj`
- **Format Code:** `dotnet format`

## 🏗️ Architecture & Conventions
- **.NET Version:** The project uses **.NET 10** (see `global.json` and `.csproj` files, despite older README references). Use .NET 10 C# features.
- **Minimal APIs:** Do not create MVC Controllers. Endpoints are registered via extension methods in `src/ALB.Api/Endpoints/` (e.g., `routeBuilder.MapChildrenEndpointsGroup()`).
- **Data Access:** Endpoints typically inject and use Repositories from the domain directly.
- **Data Types:** Uses `NodaTime` for date/time representation instead of standard `DateTime`. `UUIDv7` is used for entity primary keys.
- **Layered Design:**
  - `ALB.Domain`: Entities, Value Objects, and Repository Interfaces.
  - `ALB.Application`: Core use-cases (e.g., Authentication `TokenProvider`).
  - `ALB.Infrastructure`: EF Core `ApplicationDbContext` and Repository implementations.
- **External Clients:** Projects like `ALB.MailgunApi`, `ALB.VaultApi`, etc., are client libraries generated using **Kiota** (`Microsoft.Kiota.Bundle`).
- **Observability:** `aspire/` contains an Aspire `ServiceDefaults` project to configure standard OpenTelemetry.

## 🧪 Testing Quirks
- **Framework:** Tests use **TUnit** (NOT xUnit or NUnit). Use TUnit attributes and assertions.
- **Environment:** Always pass `-e ASPNETCORE_ENVIRONMENT=Test` when running integration tests so the correct app configuration is loaded.
- **E2E Tests:** `tests/EndToEndTests` uses Playwright + TUnit for web testing.
- **Data Generation:** Uses **Bogus** for fake entity data generation in tests.
