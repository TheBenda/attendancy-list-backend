using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Users;

internal static class GetUserRolesEndpoint
{
    internal static IEndpointRouteBuilder MapGetUserRolesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/roles", () =>
            {
                return Results.Ok(SystemRoles.Roles.Select(r => r.ToString()).ToList());
            }).WithName("GetUserRoles")
            .WithSummary("Gets all available roles.")
            .Produces<List<string>>();
        return endpoints;
    }
}