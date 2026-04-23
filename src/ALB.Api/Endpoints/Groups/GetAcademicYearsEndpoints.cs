using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups;

internal static class GetAcademicYearsEndpoints
{
    internal static IEndpointRouteBuilder MapGetAcademicYearsEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/academic-years", async (IGroupRepository groupRepository, CancellationToken ct) =>
            {
                var academicYears = await groupRepository.GetAcademicYearsAsync(ct);

                return Results.Ok(academicYears.Select(ay => new AcademicYearDto(ay.Id, ay.StartDate, ay.EndDate))
                    .ToList());
            }).WithName("GetAcademicYears")
            .Produces<List<AcademicYearDto>>()
            .RequireAuthorization(SystemRoles.AdminPolicy);
        return routeBuilder;
    }
}

public record AcademicYearDto(Guid Id, DateOnly StartDate, DateOnly EndDate);