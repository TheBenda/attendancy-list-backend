using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups;

public static class GetGroupEndpoint
{
    public static RouteGroupBuilder MapGetGroupEndpoint(this RouteGroupBuilder builder)
    {
        builder.MapGet("/{groupId:guid}", async (Guid groupId, IGroupRepository groupRepository, CancellationToken ct) =>
        {
            var group = await groupRepository.GetByIdAsync(groupId, ct);
            if (group is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new GetGroupResponse(group.Id, group.Name));
        }).WithName("GetGroup")
        .Produces<GetGroupResponse>()
        .RequireAuthorization(SystemRoles.AdminPolicy);
        
        return builder;
    }
}

public record GetGroupResponse(Guid Id, string GroupName);