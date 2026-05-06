using ALB.Api.Endpoints.Groups.Mappers;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups;

internal static class GetAllowedGroupnamesEndpoint
{
    internal static IEndpointRouteBuilder MapGetAllowedGroupnamesEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/allowed-groupnames", async (IGroupRepository groupRepository, CancellationToken ct) =>
            {
                var allowedGroupnames = await groupRepository.GetAllAllowedGroupnamesAsync(ct);
                var allowedGroupnameDtos = allowedGroupnames.Select(ag =>
                        ag.ToDto())
                    .ToList();
                return Results.Ok(new GetAllowedGroupnamesResponse(allowedGroupnameDtos));
            }).WithName("GetAllowedGroupnames")
            .Produces<GetAllowedGroupnamesResponse>()
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}

public record AllowedGroupnamesDto(Guid Id, string GroupName);

public record GetAllowedGroupnamesResponse(List<AllowedGroupnamesDto> AllowedGroupnames);