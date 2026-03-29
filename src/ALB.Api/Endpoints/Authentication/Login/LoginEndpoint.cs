using ALB.Api.Endpoints.Users.Mappers;
using ALB.Api.Models;
using ALB.Application.UseCases.Auths;
using ALB.Domain.Identity;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Authentication.Login;

internal static class LoginEndpoint
{
    internal static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/login", async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> (LoginRequest request, UserManager<ApplicationUser> userManager, TokenProvider tokenProver, CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return TypedResults.Unauthorized();
            }
            
            var userRoles = await userManager.GetRolesAsync(user);

            return TypedResults.Ok(
                new LoginResponse(
                    user.ToDto(userRoles),
                    await tokenProver.Create(user, userRoles),
                    await tokenProver.GenerateRefreshToken(user, ct)));

        }).AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            operation.Summary = "Login";
            operation.Description = "Login with email and password for JWT authentication.";
            return Task.CompletedTask;
        }).AllowAnonymous();

        return endpoints;
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(UserDto user, string AccessToken, string RefreshToken);