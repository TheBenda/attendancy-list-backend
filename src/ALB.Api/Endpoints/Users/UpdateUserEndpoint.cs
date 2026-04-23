using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Users;

internal static class UpdateUserEndpoint
{
    internal static IEndpointRouteBuilder MapUpdateUserEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPut("/{userId:guid}",
                async (Guid userId, UpdateUserRequest request, UserManager<ApplicationUser> userManager,
                    CancellationToken ct) =>
                {
                    var user = await userManager.FindByIdAsync(userId.ToString());

                    if (user is null)
                    {
                        return Results.NotFound();
                    }

                    if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;

                    if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;

                    var result = await userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description);
                        return Results.BadRequest(errors);
                    }

                    return Results.NoContent();
                }).WithName("UpdateUser")
            .WithOpenApi()
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}

public record UpdateUserRequest(string? FirstName, string? LastName);