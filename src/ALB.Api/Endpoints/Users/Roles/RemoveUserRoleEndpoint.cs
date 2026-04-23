using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Users.Roles;

internal static class RemoveUserRoleEndpoint
{
    internal static IEndpointRouteBuilder MapRemoveUserRoleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/{userId:guid}/roles",
                async (Guid userId, string role, UserManager<ApplicationUser> userManager,
                    RoleManager<ApplicationRole> roleManager, CancellationToken ct) =>
                {
                    var user = await userManager.FindByIdAsync(userId.ToString());
                    if (user is null)
                    {
                        return Results.NotFound("User not found");
                    }

                    var roleExists = await roleManager.RoleExistsAsync(role);
                    if (!roleExists)
                    {
                        return Results.NotFound("Role not found");
                    }

                    if (!await userManager.IsInRoleAsync(user, role))
                        return Results.BadRequest($"User is not in role {role}");

                    var result = await userManager.RemoveFromRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        return Results.InternalServerError(result.Errors);
                    }

                    return Results.NoContent();
                }).WithName("RemoveUserRole")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500)
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return endpoints;
    }
}