using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups.Children;

internal static class RemoveChildrenFromGroupEndpoint
{
    internal static IEndpointRouteBuilder MapRemoveChildrenFromGroupEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/{groupId:guid}/children",
                async (Guid groupId, List<Guid> ChildIds, IGroupRepository repository, CancellationToken ct) =>
                {
                    await repository.RemoveChildrenFromGroupAsync(groupId, ChildIds, ct);

                    return Results.NoContent();
                }).WithName("RemoveChildrenFromGroup")
            .WithOpenApi()
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return endpoints;
    }
}