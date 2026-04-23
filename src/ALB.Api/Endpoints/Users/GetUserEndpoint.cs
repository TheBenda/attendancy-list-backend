using ALB.Api.Endpoints.Users.Mappers;
using ALB.Api.Models;
using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Users;

internal static class GetUserEndpoint
{
    internal static IEndpointRouteBuilder MapGetUserEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/{userId:guid}", async (Guid userId, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return Results.NotFound();
            }

            var userRoles = await userManager.GetRolesAsync(user);

            var response = new GetUserResponse(user.ToDto(userRoles));

            return Results.Ok(response);
        }).WithName("GetUser")
        .Produces<GetUserResponse>()
        .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}

public record GetUserResponse(UserDto User);