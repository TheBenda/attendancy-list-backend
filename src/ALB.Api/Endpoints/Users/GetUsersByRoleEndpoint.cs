using ALB.Api.Endpoints.Users.Mappers;
using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Users;

internal static class GetUsersByRoleEndpoint
{
    internal static IEndpointRouteBuilder MapGetUsersByRoleEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/by-role", async (string userRole, UserManager<ApplicationUser> userManager) =>
            {
                var users = await userManager.GetUsersInRoleAsync(userRole);

                var userDtos = users.Select(u => u.ToDto(new List<string>())).ToList();

                return Results.Ok(userDtos.ToResponse());
            }).WithName("GetUsersByRole")
            .Produces<GetUsersResponse>()
            .RequireAuthorization(SystemRoles.AdminPolicy);
        return routeBuilder;
    }
}