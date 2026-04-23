using ALB.Domain.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ALB.Api.Endpoints.Authentication.Logout;

internal static class LogoutEndpoint
{
    internal static RouteGroupBuilder MapLogoutEndpoint(this RouteGroupBuilder routeBuilder)
    {
        routeBuilder.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager,
                [FromBody] object empty) =>
            {
                if (empty is not null)
                {
                    await signInManager.SignOutAsync();
                    return Results.Ok(new LogoutResponse("Successfully logged out", DateTime.UtcNow));
                }

                return Results.Unauthorized();
            })
            .WithOpenApi()
            .RequireAuthorization();

        return routeBuilder;
    }
}

public record LogoutResponse(string Message, DateTime LoggedOutAt);