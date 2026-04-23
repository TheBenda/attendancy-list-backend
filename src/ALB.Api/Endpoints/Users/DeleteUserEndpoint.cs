using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Users;

internal static class DeleteUserEndpoint
{
    internal static IEndpointRouteBuilder MapDeleteUserEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapDelete("/{userId:guid}",
                async (Guid userId, UserManager<ApplicationUser> userManager, CancellationToken ct) =>
                {
                    var user = await userManager.FindByIdAsync(userId.ToString());

                    if (user is null)
                    {
                        return Results.NotFound();
                    }

                    var result = await userManager.DeleteAsync(user);

                    if (!result.Succeeded)
                    {
                        return Results.BadRequest(result.Errors.AsErrorString());
                    }

                    return Results.NoContent();
                }).WithName("DeleteUser")
            .WithOpenApi()
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}