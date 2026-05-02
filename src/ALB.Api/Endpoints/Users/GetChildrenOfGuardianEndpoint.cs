using ALB.Api.Models;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Users;

internal static class GetChildrenOfGuardianEndpoint
{
    internal static IEndpointRouteBuilder MapGetChildrenOfGuardianEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/guardians-children/{guardianId:guid}", async (Guid guardianId, IChildRepository childRepository, CancellationToken ct) =>
            {
                var children = await childRepository.GetChildrenOfGuardian(guardianId, ct);

                return Results.Ok(new GetChildrenOfGuardianResponse(children.Select(c => c.toDto()).ToArray()));
            })
            .WithName("GetChildrenOfGuardian")
            .Produces<GetChildrenOfGuardianResponse>()
            .RequireAuthorization(SystemRoles.AdminPolicy);
        
        return builder;
    }
}

public record GetChildrenOfGuardianResponse(ChildDto[] Children);