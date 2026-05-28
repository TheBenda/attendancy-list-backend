using ALB.Application.UseCases.Auths;
using ALB.Domain.Entities;
using ALB.Domain.Identity;
using ALB.Domain.Repositories;
using ALB.Domain.Values;
using ALB.MailgunApi.Adapters;

namespace ALB.Api.Endpoints.Users;

internal static class InviteUserEndpoint
{
    internal static IEndpointRouteBuilder MapInviteUserEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/invite", async (InviteUserRequest request, TokenProvider tokenProvider, IInviteUsersRepository repository, IMailgunApiAdapter mailgunApiAdapter, CancellationToken ct) =>
            {
                var inviteUserId = Guid.CreateVersion7();
                var token = tokenProvider.CreateInviteToken(inviteUserId, request.Email);
                var inviteUser = new InviteUser
                {
                    Id = inviteUserId,
                    Email = request.Email,
                    Token = token,
                    FirstNames = request.FirstName,
                    LastNames = request.LastName
                };
                
                await repository.CreateAsync(inviteUser, ct);
                
                return Results.NoContent();
        }).WithName("InviteUser")
        .RequireAuthorization(SystemRoles.AdminPolicy);
        
        return routeBuilder;
    }
}
internal record InviteUserRequest(string Email, string FirstName, string LastName);