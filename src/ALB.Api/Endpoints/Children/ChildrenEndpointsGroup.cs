using ALB.Api.Endpoints.Children.Absence;

namespace ALB.Api.Endpoints.Children;

internal static class ChildrenEndpointsGroup
{
    internal static void MapChildrenEndpointsGroup(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGroup("/api/children")
            .WithTags("Children Management")
            .AddCreateChildEndpoint()
            .AddDeleteChildEndpoint()
            .AddGetChildEndpoint()
            .MapGetChildrenEndpoint()
            .AddUpdateChildEndpoint()
            .AddCreateAbsenceForChildEndpoint();
    }
}