using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups;

internal static class UpdateGroupEndpoint
{
    internal static IEndpointRouteBuilder MapUpdateGroupEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPut("/", async (UpdateGroupRequest request, IGroupRepository groupRepository, CancellationToken cancellationToken) =>
                {
                    throw new NotImplementedException();
                }
            ).WithName("UpdateGroup")
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}

public record UpdateGroupRequest(Guid groupId, string GroupName);