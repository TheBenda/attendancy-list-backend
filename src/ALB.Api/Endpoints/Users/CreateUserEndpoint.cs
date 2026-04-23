using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;

namespace ALB.Api.Endpoints.Users;

internal static class CreateUserEndpoint
{
    internal static IEndpointRouteBuilder MapCreateUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/",
                async (CreateUserRequest request, UserManager<ApplicationUser> userManager, CancellationToken ct) =>
                {
                    var user = new ApplicationUser
                    {
                        Email = request.Email,
                        UserName = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName
                    };

                    var result = await userManager.CreateAsync(user, request.Password);

                    if (!result.Succeeded)
                    {
                        return Results.BadRequest(result.Errors.AsErrorString());
                    }

                    return Results.Ok(new CreateUserResponse(user.Id, user.Email, user.FirstName, user.LastName));
                }).WithName("CreateUser")
            .Produces<CreateUserResponse>()
            .ProducesProblem(400)
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return endpoints;
    }
}

public record CreateUserRequest(string Email, string Password, string? FirstName, string? LastName);

public record CreateUserResponse(Guid Id, string Email, string? FirstName, string? LastName);