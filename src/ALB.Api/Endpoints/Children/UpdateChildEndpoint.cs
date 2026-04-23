using ALB.Domain.Repositories;
using ALB.Domain.Values;

using NodaTime;

namespace ALB.Api.Endpoints.Children;

internal static class UpdateChildEndpoint
{
    internal static IEndpointRouteBuilder AddUpdateChildEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{childId:guid}",
                async (Guid childId, UpdateChildRequest request, IChildRepository childRepository) =>
                {
                    var existingChild = await childRepository.GetByIdAsync(childId);
                    if (existingChild is null)
                    {
                        return Results.NotFound();
                    }

                    existingChild.FirstName = request.ChildFirstName;
                    existingChild.LastName = request.ChildLastName;
                    existingChild.DateOfBirth = request.ChildDateOfBirth;

                    await childRepository.UpdateAsync(existingChild);

                    return Results.NoContent();
                }).WithName("UpdateChild")
            .WithOpenApi()
            .RequireAuthorization(policy => policy.RequireRole(SystemRoles.Admin));
        return builder;
    }
}

public record UpdateChildRequest(string ChildFirstName, string ChildLastName, LocalDate ChildDateOfBirth);