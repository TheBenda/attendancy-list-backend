using ALB.Domain.Options;

using Microsoft.FeatureManagement;

namespace ALB.Api.Endpoints;

internal static class GetEnabledFeaturesEndpoint
{
    internal static IEndpointRouteBuilder MapGetEnabledFeaturesEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/", async (IFeatureManager featureManager) => 
                Results.Ok(new GetEnabledFeaturesResponse(await featureManager.IsEnabledAsync(FeatureFlags.InviteUsers))))
            .WithTags("Features")
            .Produces<GetEnabledFeaturesResponse>()
            .AllowAnonymous();
        return routeBuilder;
    }
}

internal record GetEnabledFeaturesResponse(
    bool UserInvitationEnabled);