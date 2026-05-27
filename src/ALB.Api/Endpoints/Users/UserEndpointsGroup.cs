using ALB.Api.Endpoints.Users.Roles;

namespace ALB.Api.Endpoints.Users;

internal static class UserEndpointsGroup
{
    internal static void MapUserEndpointsGroup(this IEndpointRouteBuilder routeBuilder, string environment)
    {
        var userEndpointGroup = routeBuilder.MapGroup("/api/users")
            .WithTags("Users Management");
        
        userEndpointGroup
            .MapCreateUserEndpoint()
            .MapDeleteUserEndpoint()
            .MapGetUsersEndpoint()
            .MapGetUserRolesEndpoint()
            .MapGetUsersByRoleEndpoint()
            .MapGetUserEndpoint()
            .MapGetChildrenOfGuardianEndpoint()
            .MapUpdateUserEndpoint()
            .MapRemoveUserRoleEndpoint()
            .MapGetInvitedUserEndpoint()
            .MapRegisterInvitedUserEndpoint()
            .MapAddUserRoleEndpoint();
        
        
        if (environment != "Test")
        {
            userEndpointGroup.MapInviteUserEndpoint();
        }
    }
}