using ALB.Api.Endpoints.Groups.Children;
using ALB.Api.Endpoints.Groups.Cohorts;

namespace ALB.Api.Endpoints.Groups;

internal static class GroupsEndpointsGroup
{
    internal static void MapGroupsEndpointsGroup(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGroup("/api/groups")
            .WithTags("Groups Management")
            .MapAddChildrenToGroupEndpoint()
            .MapRemoveChildrenFromGroupEndpoint()
            .MapCreateCohortEndpoint()
            .MapCreateGroupEndpoint()
            .MapDeleteGroupEndpoint()
            .MapUpdateGroupEndpoint();
    }
}