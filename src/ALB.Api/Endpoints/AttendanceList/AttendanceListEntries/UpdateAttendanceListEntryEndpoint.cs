using ALB.Application;
using ALB.Domain.Enum;
using ALB.Domain.Repositories;

using NodaTime;

namespace ALB.Api.Endpoints.AttendanceList.AttendanceListEntries;

internal static class UpdateAttendanceListEntryEndpoint
{
    internal static RouteGroupBuilder AddUpdateAttendanceListEntryEndpoint(this RouteGroupBuilder builder)
    {
        builder.MapPut("/entries", async (UpdateAttendanceListEntryRequest request, IAttendanceRepository repository) =>
        {
            await repository.UpdateAsync(request.ChildId, LocalDate.FromDateTime(request.Date),
                request.ArrivalAt.ToNodaLocalTime(), request.DepartureAt.ToNodaLocalTime(), request.Status,
                CancellationToken.None);

            return Results.Ok(new UpdateAttendanceListEntryResponse(
                $"Attendance for {request.ChildId} at {LocalDate.FromDateTime(request.Date)} was successfully set to {request.Status}"));
        }).WithName("UpdateAttendanceListEntry").WithOpenApi().AllowAnonymous();

        return builder;
    }
}

public record UpdateAttendanceListEntryRequest(
    Guid ChildId,
    DateTime Date,
    DateTime ArrivalAt,
    DateTime DepartureAt,
    AttendanceStatus Status);

public record UpdateAttendanceListEntryResponse(string Message);