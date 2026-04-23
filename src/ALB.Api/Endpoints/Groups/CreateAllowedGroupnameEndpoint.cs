using ALB.Domain.Entities;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Mvc;

namespace ALB.Api.Endpoints.Groups;

internal static class CreateAllowedGroupnameEndpoint
{
    internal static IEndpointRouteBuilder MapCreateAllowedGroupnameEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/allowed-groupnames",
                async (CreateAllowedGroupnameRequest request, IGroupRepository groupRepository, CancellationToken cancellationToken) =>
                {
                    var allowedGroupname = new AllowedGroupname { Groupname = request.GroupName };
                    await groupRepository.CreateAllowedGroupnameAsync(allowedGroupname, cancellationToken);
                    return Results.NoContent();
                }
            ).WithName("CreateAllowedGroupname")
            .Produces<NoContentResult>()
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}

public record CreateAllowedGroupnameRequest(string GroupName);