using System.Security.Claims;

namespace ALB.Api.Endpoints.Authentication.Login;

internal static class RevokeRefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRevokeRefreshTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/revoke-refresh-token",
                async (ClaimsPrincipal claims, CancellationToken cancellationToken) =>
                {
                    var identies = claims.Identities;
                }).WithOpenApi()
            .RequireAuthorization();

        return endpoints;
    }
}