using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Children;

internal static class GetChildrenEndpoint
{
    internal static IEndpointRouteBuilder MapGetChildrenEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/",
                async (IChildRepository repository, CancellationToken ct, Guid? cursor = null, int limit = 10) =>
                {
                    switch (limit)
                    {
                        case < 1:
                            return Results.BadRequest("Limit must be greater than 0");
                        case > 100:
                            return Results.BadRequest("Limit must be less than 100");
                    }

                    var children = await repository
                        .TakeChildrenByCursor(cursor, limit, ct);

                    var hasMore = children.Count > limit;

                    Guid? nextCursor = hasMore ? children[^1].Id : null;

                    if (hasMore) children.RemoveAt(children.Count - 1);

                    var response = new GuidCursorResponse<GetChildResponse>(
                        children.Select(c => c.ToResponse()).ToList(),
                        new GuidCursorRequest(nextCursor, limit),
                        hasMore
                    );
                    return Results.Ok(response);
                }).WithName("GetChildren")
            .Produces<GuidCursorResponse<GetChildResponse>>()
            .ProducesProblem(400)
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return builder;
    }
}