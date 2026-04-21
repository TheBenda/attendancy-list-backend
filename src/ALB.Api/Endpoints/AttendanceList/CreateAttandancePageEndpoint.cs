using ALB.Domain.Repositories;
using ALB.Domain.Values;

using NodaTime;

namespace ALB.Api.Endpoints.AttendanceList;

internal static class GetAttendancePageEndpoint
{
    internal static RouteGroupBuilder AddAttendancePageEndpoint(this RouteGroupBuilder builder)
    {
        builder.MapPost("/attendancelists/{attendanceListId:guid}/page", async (Guid attendanceListId, GetAttendancePageRequest request, IAttendanceRepository attendanceListRepo, IChildRepository childRepo, IAbsenceDayRepository absenceRepo) =>
        {
            return Results.InternalServerError();
        }).WithName("GetAttendancePage")
            .RequireAuthorization(SystemRoles.TeamPolicy);
        return builder;
    }
}

public record GetAttendancePageRequest(LocalDate date);

public record AttendancePageChildDto(Guid ChildId, string FirstName, string LastName, string Status);

public record GetAttendancePageResponse(List<AttendancePageChildDto> Children);