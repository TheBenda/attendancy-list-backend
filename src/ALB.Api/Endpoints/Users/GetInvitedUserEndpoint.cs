using ALB.Domain.Entities;
using ALB.Domain.Repositories;

namespace ALB.Api.Endpoints.Users;

internal static class GetInvitedUserEndpoint
{
    internal static IEndpointRouteBuilder MapGetInvitedUserEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/invited-user/{id:guid}", async (Guid id, IInviteUsersRepository repository) =>
            {
                var user = await repository.GetByIdAsync(id);
                if (user is null)
                {
                    return Results.NotFound("User not found.");
                }

                return Results.Ok(user.ToDto());
            }).WithName("GetInvitedUser")
            .Produces<GetInvitedUserDto>()
            .ProducesProblem(404)
            .AllowAnonymous();
        
        return routeBuilder;
    }

    internal static GetInvitedUserDto ToDto(this InviteUser user)
        => new GetInvitedUserDto(user.Id, user.Email, user.FirstNames, user.LastNames, user.Token);
}

public record GetInvitedUserDto(Guid Id, string Email, string FirstName, string LastName, string Token);