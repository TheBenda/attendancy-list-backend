using ALB.Api.Endpoints.Users.Roles;

namespace ALB.Api.Endpoints.Users;

internal static class UserEndpointsGroup
{
    internal static void MapUserEndpointsGroup(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGroup("/api/users")
            .WithTags("Users Management")
            .MapCreateUserEndpoint()
            .MapDeleteUserEndpoint()
            .MapGetUsersEndpoint()
            .MapGetUserRolesEndpoint()
            .MapGetUsersByRoleEndpoint()
            .MapGetUserEndpoint()
            .MapUpdateUserEndpoint()
            .MapAddUserRoleEndpoint();
    }
}