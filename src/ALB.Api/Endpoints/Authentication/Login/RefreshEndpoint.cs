using ALB.Application.UseCases.Auths;
using ALB.Domain.Identity;
using ALB.Domain.Repositories;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Authentication.Login;

internal static class RefreshEndpoint
{
    internal static IEndpointRouteBuilder MapRefreshEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/refresh", async (RefreshRequest refreshRequest, UserManager<ApplicationUser> userManager, IRefreshTokenRepository repository, TokenProvider tokenProvider, CancellationToken ct) =>
        {
            var refreshToken = await repository.FindByRefreshTokenAsync(refreshRequest.RefreshToken, ct);

            if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
            {
                return Results.BadRequest("The refresh token has expired.");
            }

            var userRoles = await userManager.GetRolesAsync(refreshToken.User);

            return Results.Ok(
                new
                {
                    AccessToken = await tokenProvider.Create(refreshToken.User, userRoles),
                    RefreshToken = await tokenProvider.UpdateTokenExpiration(refreshToken.Id, ct)
                });
        }).AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            operation.Summary = "Refresh";
            operation.Description = "Get a new access token using a refresh token.";
            return Task.CompletedTask;
        })
        .RequireAuthorization();

        return endpoints;
    }
}

public record RefreshRequest(string RefreshToken);