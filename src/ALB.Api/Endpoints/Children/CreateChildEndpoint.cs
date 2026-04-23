using ALB.Api.Endpoints.Users.Mappers;
using ALB.Api.Models;
using ALB.Domain.Entities;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using NodaTime;

namespace ALB.Api.Endpoints.Children;

internal static class CreateChildEndpoint
{
    internal static IEndpointRouteBuilder AddCreateChildEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", async (CreateChildRequest request, IChildRepository childRepository, CancellationToken ct) =>
        {
            var child = new Child
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth
            };

            var createdChild = await childRepository.CreateAsync(child, ct);

            if (request.GuardianIds.Count != 0)
            {
                await childRepository.AddGuardiansToChildAsync(createdChild.Id, request.GuardianIds, ct);
            }

            return Results.Ok(new CreateChildResponse(createdChild.Id,
                createdChild.FirstName,
                createdChild.LastName,
                createdChild.DateOfBirth,
                createdChild.Guardians.Select(g => g.ToDto([])).ToList()));
        }).WithName("CreateChild")
            .RequireAuthorization(policy => policy.RequireRole(SystemRoles.Admin));
        return builder;
    }
}

public record CreateChildRequest(string FirstName, string LastName, LocalDate DateOfBirth, List<Guid> GuardianIds);

public record CreateChildResponse(Guid Id, string FirstName, string LastName, LocalDate DateOfBirth, List<UserDto> Guardians);