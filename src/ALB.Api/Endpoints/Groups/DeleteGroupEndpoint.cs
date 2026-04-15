using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups;

internal static class DeleteGroupEndpoint
{
    internal static IEndpointRouteBuilder MapDeleteGroupEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapDelete("/{groupId:guid}",
            async (Guid groupId, IGroupRepository groupRepository, CancellationToken ct) =>
                await groupRepository.DeleteAsync(groupId)
        ).WithName("DeleteGroup")
        .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}