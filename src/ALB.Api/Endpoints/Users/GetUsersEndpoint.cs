using ALB.Api.Endpoints.Users.Mappers;
using ALB.Api.Models;
using ALB.Domain.Identity;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ALB.Api.Endpoints.Users;

internal static class GetUsersEndpoint
{
    internal static IEndpointRouteBuilder MapGetUsersEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/", async (UserManager<ApplicationUser> userManager, CancellationToken ct) =>
            {
                var userDtos = await userManager.Users.Select(u => u.ToDto(new List<string>())).ToListAsync(ct);

                return Results.Ok(userDtos.ToResponse());
            }).WithName("GetUsers")
            .Produces<GetUsersResponse>()
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}

public record GetUsersResponse(List<UserDto> Users);