using ALB.Domain.Entities;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using NodaTime;

namespace ALB.Api.Endpoints.Children;

internal static class GetChildEndpoint
{
    internal static IEndpointRouteBuilder AddGetChildEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{childId:guid}", async (Guid childId, IChildRepository childRepository) =>
        {
            var child = await childRepository.GetByIdAsync(childId);

            if (child is null)
            {
                return Results.NotFound();
            }

            var response = new GetChildResponse(
                child.Id,
                child.FirstName,
                child.LastName,
                child.DateOfBirth
            );

            return Results.Ok(response);
        }).WithName("GetChild")
            .RequireAuthorization(policy => policy.RequireRole(SystemRoles.Admin));

        return builder;
    }
}

public record GetChildResponse(Guid Id, string FirstName, string LastName, LocalDate DateOfBirth);