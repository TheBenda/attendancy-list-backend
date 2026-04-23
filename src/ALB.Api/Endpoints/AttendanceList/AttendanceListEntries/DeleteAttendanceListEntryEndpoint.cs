using ALB.Domain.Repositories;

using NodaTime;

namespace ALB.Api.Endpoints.AttendanceList.AttendanceListEntries;

internal static class DeleteAttendanceListEntryEndpoint
{
    internal static RouteGroupBuilder AddDeleteAttendanceListEntryEndpoint(this RouteGroupBuilder builder)
    {
        builder.MapDelete("/entries",
            async (DeleteAttendanceListEntryRequest request, IAttendanceRepository repository) =>
            {
                await repository.DeleteAsync(request.ChildId, LocalDate.FromDateTime(request.Date),
                    CancellationToken.None);

                return Results.Ok(new DeleteAttendanceListEntryResponse(
                    $"Attendance for {request.ChildId} at {request.Date} was successfully deleted."));
            }).WithName("DeleteAttendanceListEntry").WithOpenApi().AllowAnonymous();

        return builder;
    }
}

public record DeleteAttendanceListEntryRequest(Guid ChildId, DateTime Date);

public record DeleteAttendanceListEntryResponse(string Message);