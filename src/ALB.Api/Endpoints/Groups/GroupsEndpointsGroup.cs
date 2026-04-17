using ALB.Api.Endpoints.Groups.Children;
using ALB.Api.Endpoints.Groups.Cohorts;

namespace ALB.Api.Endpoints.Groups;

internal static class GroupsEndpointsGroup
{
    internal static void MapGroupsEndpointsGroup(this IEndpointRouteBuilder routeBuilder)
    {
        //  An Admin should be able to see all groups. In a new folder 'groups' under 'views', create a new view called 'GroupeManagement.vue'.
        routeBuilder.MapGroup("/api/groups")
            .WithTags("Groups Management")
            .MapAddChildrenToGroupEndpoint()
            .MapGetGroupsEndpoint()
            //.MapRemoveChildrenFromGroupEndpoint()
            .MapCreateCohortEndpoint()
            .MapCreateGroupEndpoint()
            .MapDeleteGroupEndpoint()
            .MapUpdateGroupEndpoint()
            .MapCreateAllowedGroupnameEndpoint()
            .MapGetAllowedGroupnamesEndpoint()
            .MapGetAcademicYearsEndpoints();
    }
}