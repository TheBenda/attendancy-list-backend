using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups;

public static class GetGroupsEndpoint
{
        internal static IEndpointRouteBuilder MapGetGroupsEndpoint(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapGet("/", async (IGroupRepository groupRepository, CancellationToken ct) =>
            {
                var groups = await groupRepository.GetAllAsync(ct);
                var groupDtos = groups.Select(g =>
                        new GetGroupResponse(g.Id, g.Name))
                    .ToList();
                return Results.Ok(new GetGroupsResponse(groupDtos));
            }).WithName("GetGroups")
            .Produces<GetGroupsResponse>()
            .RequireAuthorization(SystemRoles.AdminPolicy);
    
            return routeBuilder;
        }
}

public record GetGroupsResponse(List<GetGroupResponse> groups);