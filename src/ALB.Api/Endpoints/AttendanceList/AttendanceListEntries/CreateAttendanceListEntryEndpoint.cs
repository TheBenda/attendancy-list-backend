using ALB.Application;
using ALB.Domain.Enum;
using ALB.Domain.Repositories;

using NodaTime;

namespace ALB.Api.Endpoints.AttendanceList.AttendanceListEntries;

internal static class CreateAttendanceListEntryEndpoint
{
    internal static RouteGroupBuilder AddCreateAttendanceListEntryEndpoint(this RouteGroupBuilder builder)
    {
        builder.MapPost("/entries",
            async (CreateAttendanceListEntryRequest request, IAttendanceRepository repository) =>
            {
                await repository.CreateAsync(request.ChildId, LocalDate.FromDateTime(request.Date),
                    request.ArrivalAt.ToNodaLocalTime(), request.DepartureAt.ToNodaLocalTime(), request.Status,
                    CancellationToken.None);

                return Results.Ok(new CreateAttendanceListEntryResponse(
                    $"Attendance for {request.ChildId} at {LocalDate.FromDateTime(request.Date)} was successfully set to {request.Status}"));
            }).WithName("CreateAttendanceListEntry").WithOpenApi().AllowAnonymous();

        return builder;
    }
}

public record CreateAttendanceListEntryRequest(
    Guid ChildId,
    DateTime Date,
    DateTime ArrivalAt,
    DateTime DepartureAt,
    AttendanceStatus Status);

public record CreateAttendanceListEntryResponse(string Message);