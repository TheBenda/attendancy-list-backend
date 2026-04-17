# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands
- **Build**: `dotnet build`
- **Restore dependencies**: `dotnet restore`
- **Format code**: `dotnet format`
- **Run API**: `dotnet run --project src/ALB.Api/ALB.Api.csproj`
- **Run all tests**: `dotnet test tests/ApiIntegrationTests/ApiIntegrationTests/ApiIntegrationTests.csproj -e ASPNETCORE_ENVIRONMENT=Test` (Requires Docker to be running for Testcontainers)
- **Run a single test**: `dotnet test tests/ApiIntegrationTests/ApiIntegrationTests/ApiIntegrationTests.csproj --filter "TestName=<YourTestName>" -e ASPNETCORE_ENVIRONMENT=Test`
- **Add EF Core Migration**: `dotnet ef migrations add <MigrationName> --project src/ALB.Infrastructure/ALB.Infrastructure.csproj --startup-project src/ALB.Api/ALB.Api.csproj`
- **Update Database**: `dotnet ef database update --project src/ALB.Infrastructure/ALB.Infrastructure.csproj --startup-project src/ALB.Api/ALB.Api.csproj`

## Architecture & Structure
This is a .NET 10 backend project for an Attendance List system, utilizing a layered architecture:

- **`src/ALB.Domain`**: Core domain entities (e.g., `Child`, `Group`, `AttendanceList`), ValueObjects, and repository interfaces. Uses `NodaTime` for robust date/time representation.
- **`src/ALB.Application`**: Application layer, currently focusing on use cases like Authentication (`TokenProvider`).
- **`src/ALB.Infrastructure`**: Persistence and external services. Contains the EF Core `ApplicationDbContext`, PostgreSQL configuration, and Repository implementations.
- **`src/ALB.Api`**: The presentation layer using Minimal APIs. Endpoints are organized by feature inside `src/ALB.Api/Endpoints/`.
- **`tests/ApiIntegrationTests`**: Integration testing project that tests against a real database instance using Testcontainers.

### Key Technologies & Patterns
- **Minimal APIs**: Endpoints are built as extension methods on `IEndpointRouteBuilder` (e.g., `MapGetChildrenEndpoint`) and directly use Repositories from the domain.
- **Data Access**: Entity Framework Core with PostgreSQL. UUIDv7 is used for primary keys (`UuiDv7Generator`).
- **Testing**: Integration tests are written using **TUnit** (rather than xUnit/NUnit), **Testcontainers.PostgreSql**, and **Bogus** for fake data generation. Docker must be running to execute the tests.
- **Observability**: An Aspire `ServiceDefaults` project is included to configure OpenTelemetry and standard .NET diagnostics defaults.