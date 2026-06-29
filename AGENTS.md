# OpenCode / AI Agent Instructions

This repository is a .NET 10 Minimal API backend using EF Core, PostgreSQL, and .NET Aspire.

## Core Commands
- **Solution:** Uses `.slnx` format (`AttendanceListBackend.slnx`).
- **Add dependencies:** Always use `dotnet add package` -- centralized package management (`Directory.Packages.props`) will reject version conflicts if you edit csproj directly.
- **Run API:** `dotnet run --project src/ALB.Api/ALB.Api.csproj`
- **Run Integration Tests:** `dotnet test tests/ApiIntegrationTests/ApiIntegrationTests/ApiIntegrationTests.csproj -e ASPNETCORE_ENVIRONMENT=Test`
  - Docker MUST be running locally (uses `Testcontainers.PostgreSql`).
- **Run single test:** `dotnet test tests/ApiIntegrationTests/ApiIntegrationTests/ApiIntegrationTests.csproj --filter "TestName=<YourTestName>" -e ASPNETCORE_ENVIRONMENT=Test`
- **Add Migration:** `dotnet ef migrations add <Name> --project src/ALB.Infrastructure/ALB.Infrastructure.csproj --startup-project src/ALB.Api/ALB.Api.csproj`
- **Update Database:** `dotnet ef database update --project src/ALB.Infrastructure/ALB.Infrastructure.csproj --startup-project src/ALB.Api/ALB.Api.csproj`
- **Format Code:** `dotnet format`
- **CI order:** restore -> build -> integration tests -> format. E2E tests only run on `workflow_dispatch`.

## Architecture & Conventions
- **.NET Version:** .NET 10 (`global.json`). Use .NET 10 C# features.
- **Minimal APIs only.** No MVC controllers. Endpoints are extension methods on `IEndpointRouteBuilder` in `src/ALB.Api/Endpoints/`.
- **Endpoint wiring:** Two-tier pattern:
  1. `EndpointsExtension.MapEndpointsAsync()` dispatches to per-feature group methods (e.g., `MapChildrenEndpointsGroup()`).
  2. Each group creates a `MapGroup("/api/...")` and chains individual endpoint methods (e.g., `AddCreateChildEndpoint()`).
  3. Request/response records are co-located in the same file as their endpoint.
  4. Some groups use async registration for feature-flag gating via `IFeatureManager`.
- **Data Access:** Endpoints inject repository interfaces directly (no service/mediator layer). Repositories are defined in `ALB.Domain/Repositories/`, implemented in `ALB.Infrastructure/Persistence/Repositories/`.
- **Data Types:**
  - `NodaTime` (`LocalDate`, `LocalTime`) instead of `DateTime`/`DateTimeOffset`. Configured at both Npgsql DataSource (`UseNodaTime()`) and EF Core level.
  - Dates over the wire are Unix timestamps (long/seconds), converted via `NodaTimeMapper` extension methods in `Endpoints/Mappers/`.
  - `UUIDv7` for all entity primary keys, configured via `UuiDv7Generator` value generator in `ApplicationDbContext.OnModelCreating`.
- **Pagination:** Cursor-based using `GuidCursorRequest`/`GuidCursorResponse<T>` (`Endpoints/Paging.cs`).
- **Specifications:** Uses `Ardalis.Specification` for query specifications (e.g., `ChildrenByFirstOrLastnameSpec`).
- **Layered Design:**
  - `ALB.Domain`: Entities, Enums, Value Objects, Repository interfaces, Adapter interfaces, Options classes.
  - `ALB.Application`: Use-cases (Authentication `TokenProvider`).
  - `ALB.Infrastructure`: EF Core `ApplicationDbContext`, Repositories, Identity auth setup, `PowerUserSeederService`, background jobs.
  - `ALB.Api`: Minimal API endpoints, OpenAPI/Scalar config, NodaTime JSON converters, CORS.
- **Identity:** ASP.NET Core Identity with custom `ApplicationUser`/`ApplicationRole` (Guid-based). Custom `MapIdentityApiFilterable<T>()` selectively exposes Identity endpoints.
- **Feature Flags:** `UseMailpit` and `InviteUsers` via `Microsoft.FeatureManagement`. Controls endpoint registration and service wiring.
- **External Clients:** `ALB.MailgunApi` and `ALB.VaultApi` use **Kiota** (`Microsoft.Kiota.Bundle`) for generated API clients, wrapped with hand-written adapter classes. `ALB.SchulferienApi` and `ALB.MehrSchulferienApi` exist but are incomplete/empty and not in the solution.
- **API docs:** OpenAPI 3.1 + Scalar UI at `/api-reference` (not Swagger).
- **Migrations auto-run** at API startup via `context.Database.MigrateAsync()` in `Program.cs`.
- **Aspire:** `aspire/ALB.AppHost` orchestrates PostgreSQL, Zitadel (identity provider), Vault, Mailpit, the API, and a Vite frontend. Uses Kubernetes with Traefik gateway (`*.localtest.me` domains).

## Testing Quirks
- **Framework:** Tests use **TUnit** (NOT xUnit or NUnit). Use `[Test]` attribute and `await Assert.That(...)` fluent assertions.
- **Environment:** Always pass `-e ASPNETCORE_ENVIRONMENT=Test`. The test factory uses `builder.UseEnvironment("Test")`.
- **Test setup:** `BaseIntegrationTest` implements TUnit's `IAsyncInitializer`/`IAsyncDisposable`. Spins up a `PostgreSqlContainer`, creates a `WebApplicationFactory<IApiAssemblyMarker>`, runs migrations, and seeds users. Shared per-assembly via `[ClassDataSource<BaseIntegrationTest>(Shared = SharedType.PerAssembly)]`.
- **Auth in tests:** JWT Bearer is replaced with a `TestAuthHandler` that reads an `X-Api-Key` header to assign roles (Admin/Parent). No real JWT tokens needed in tests.
- **NodaTime in tests:** Use the custom `HttpClientExtensions` methods (`PostAsJsonAsync`, `GetFromJsonAsync`, etc.) that configure NodaTime JSON serialization -- do not use the default `System.Net.Http.Json` extensions directly.
- **Data Generation:** Uses **Bogus** fakers in `TestDataFaker` for `CreateChildRequest`/`CreateUserRequest`.
- **E2E Tests:** `tests/EndToEndTests` uses Playwright + TUnit + Aspire `DistributedApplicationTestingBuilder` (full-stack).
