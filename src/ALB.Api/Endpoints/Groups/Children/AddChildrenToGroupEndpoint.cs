using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups.Children;

internal static class AddChildrenToGroupEndpoint
{
    internal static IEndpointRouteBuilder MapAddChildrenToGroupEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{groupId:guid}/children", async (Guid groupId, AddChildToGroupRequest request, IGroupRepository repository, CancellationToken cancellationToken) =>
        {
            await repository.AddChildrenToGroupAsync(groupId, request.ChildIds, cancellationToken);

            return Results.Ok();
        }).WithName("Add children to group")
        .RequireAuthorization(policy => policy.RequireRole(SystemRoles.Admin));

        return endpoints;
    }
}

public record AddChildToGroupRequest(List<Guid> ChildIds);