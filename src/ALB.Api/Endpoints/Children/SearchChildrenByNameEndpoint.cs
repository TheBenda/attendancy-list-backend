using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Repositories;
using ALB.Domain.Specifications;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Children;

internal static class SearchChildrenByNameEndpoint
{
    internal static IEndpointRouteBuilder MapSearchChildrenByNameEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/search-by-name",
            async (IChildRepository repository, CancellationToken ct, string name, Guid? cursor = null,
                int limit = 10) =>
            {
                switch (limit)
                {
                    case < 1:
                        return Results.BadRequest("Limit must be greater than 0");
                    case > 100:
                        return Results.BadRequest("Limit must be less than 100");
                }
                
                var spec = new ChildrenByFirstOrLastnameSpec(cursor, limit, name);

                var searchResult = await repository.ListChildrenAsync(spec);
                
                var hasMore = searchResult.Count > limit;

                Guid? nextCursor = hasMore ? searchResult[^1].Id : null;

                if (hasMore) searchResult.RemoveAt(searchResult.Count - 1);

                var response = new GuidCursorResponse<GetChildResponse>(
                    searchResult.Select(c => c.ToResponse()).ToList(),
                    new GuidCursorRequest(nextCursor, limit),
                    hasMore
                );
                return Results.Ok(response);
            }).WithName("SearchChildrenByName")
            .Produces<GuidCursorResponse<GetChildResponse>>()
            .ProducesProblem(400)
            .RequireAuthorization([SystemRoles.AdminPolicy, SystemRoles.CoAdminPolicy]);
        
        return builder;
    }
}