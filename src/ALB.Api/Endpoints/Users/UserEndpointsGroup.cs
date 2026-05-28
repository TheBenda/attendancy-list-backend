using ALB.Api.Endpoints.Users.Roles;
using ALB.Domain.Options;

using Microsoft.FeatureManagement;

namespace ALB.Api.Endpoints.Users;

internal static class UserEndpointsGroup
{
    internal static async Task MapUserEndpointsGroupAsync(this IEndpointRouteBuilder routeBuilder, IFeatureManager featureManager)
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
            .MapAddUserRoleEndpoint();
        
        if (await featureManager.IsEnabledAsync(FeatureFlags.InviteUsers))
        {
            userEndpointGroup.MapInviteUserEndpoint()
                .MapGetInvitedUserEndpoint()
                .MapRegisterInvitedUserEndpoint();
        }
    }
}