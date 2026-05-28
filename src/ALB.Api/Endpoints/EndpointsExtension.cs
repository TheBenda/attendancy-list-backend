using ALB.Api.Endpoints.AttendanceList;
using ALB.Api.Endpoints.Authentication;
using ALB.Api.Endpoints.Children;
using ALB.Api.Endpoints.Groups;
using ALB.Api.Endpoints.Users;
using Microsoft.FeatureManagement;

namespace ALB.Api.Endpoints;

internal static class EndpointsExtension
{
    internal static async Task MapEndpointsAsync(this IEndpointRouteBuilder routeBuilder, IFeatureManager featureManager)
    {
        var group = routeBuilder.MapGroup("/api/features")
            .WithTags("Features");

        group.MapGetEnabledFeaturesEndpoint();
        
        routeBuilder.MapAuthEndpointsGroup();
        routeBuilder.MapAttendanceListEndpointsGroup();
        routeBuilder.MapChildrenEndpointsGroup();
        await routeBuilder.MapUserEndpointsGroupAsync(featureManager);
        routeBuilder.MapGroupsEndpointsGroup();
    }
}