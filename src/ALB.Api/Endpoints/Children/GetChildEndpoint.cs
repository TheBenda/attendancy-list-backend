using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Children;

internal static class GetChildEndpoint
{
    internal static IEndpointRouteBuilder AddGetChildEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{childId:guid}", async (Guid childId, IChildRepository childRepository) =>
            {
                var child = await childRepository.GetByIdAsync(childId);

                return child is null ? Results.NotFound() : Results.Ok(child.ToResponse());
            }).WithName("GetChild")
            .RequireAuthorization(policy => policy.RequireRole(SystemRoles.Admin));

        return builder;
    }
}

public record GetChildResponse(Guid Id, string FirstName, string LastName, long DateOfBirth);