FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-level files for restore (global.json pins SDK version)
COPY global.json .
COPY Directory.Packages.props .
COPY nuget.config .

# Copy all project files for restore
COPY src/ALB.Api/ALB.Api.csproj src/ALB.Api/
COPY src/ALB.Application/ALB.Application.csproj src/ALB.Application/
COPY src/ALB.Domain/ALB.Domain.csproj src/ALB.Domain/
COPY src/ALB.Infrastructure/ALB.Infrastructure.csproj src/ALB.Infrastructure/
COPY src/ALB.MailgunApi/ALB.MailgunApi.csproj src/ALB.MailgunApi/
COPY src/ALB.VaultApi/ALB.VaultApi.csproj src/ALB.VaultApi/
COPY aspire/ServiceDefaults/ServiceDefaults.csproj aspire/ServiceDefaults/

RUN dotnet restore src/ALB.Api/ALB.Api.csproj

# Copy all source and build
COPY src/ src/
COPY aspire/ServiceDefaults/ aspire/ServiceDefaults/

RUN dotnet publish src/ALB.Api/ALB.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ALB.Api.dll"]
