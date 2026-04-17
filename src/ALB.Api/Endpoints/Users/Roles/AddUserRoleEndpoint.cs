using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Users.Roles;

internal static class AddUserRoleEndpoint
{
    internal static IEndpointRouteBuilder MapAddUserRoleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{userId:guid}/roles", async (Guid userId, AddUserRoleRequest request, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, CancellationToken ct) =>
        {
            var userToAssignRoleTo = await userManager.FindByIdAsync(userId.ToString());

            if (userToAssignRoleTo is null)
            {
                return Results.NotFound("User not found");
            }

            var roleExists = await roleManager.RoleExistsAsync(request.Role);
            if (!roleExists)
            {
                return Results.NotFound("Role not found");
            }

            var result = await userManager.AddToRoleAsync(userToAssignRoleTo, request.Role);

            if (!result.Succeeded)
            {
                return Results.InternalServerError(result.Errors.AsErrorString());
            }

            return Results.NoContent();
        }).WithName("AddUserRole")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(500)
        .RequireAuthorization(SystemRoles.AdminPolicy);

        return endpoints;
    }
}

public record AddUserRoleRequest(string Role);