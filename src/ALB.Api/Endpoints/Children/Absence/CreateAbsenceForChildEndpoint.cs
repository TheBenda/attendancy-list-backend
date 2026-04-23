using ALB.Domain.Entities;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using NodaTime;

namespace ALB.Api.Endpoints.Children.Absence;

internal static class CreateAbsenceForChildEndpoint
{
    internal static IEndpointRouteBuilder AddCreateAbsenceForChildEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{childId:guid}/absence",
                async (Guid childId, CreateAbsenceRequest request, IAbsenceDayRepository absenceRepo) =>
                {
                    var startDate = LocalDate.FromDateTime(request.StartDate);
                    var endDate = LocalDate.FromDateTime(request.EndDate);

                    if (startDate > endDate)
                    {
                        return Results.BadRequest(new CreateAbsenceResponse("Start date cannot be after end date."));
                    }

                    var alreadyExists =
                        await absenceRepo.ExistsInRangeAsync(childId, startDate, endDate, CancellationToken.None);

                    if (alreadyExists)
                    {
                        return Results.Conflict(
                            new CreateAbsenceResponse(
                                "An absence already exists for one or more days in this date range."));
                    }

                    var absencesToCreate = new List<AbsenceDay>();
                    for (var date = startDate; date <= endDate; date = date.PlusDays(1))
                    {
                        var absence = new AbsenceDay
                        {
                            ChildId = childId, Date = date, AbsenceStatusId = request.AbsenceStatusId,
                        };
                        absencesToCreate.Add(absence);
                    }


                    if (absencesToCreate.Any())
                    {
                        await absenceRepo.AddRangeAsync(absencesToCreate, CancellationToken.None);
                    }

                    return Results.Ok(
                        new CreateAbsenceResponse(
                            $"Absence registered successfully for {absencesToCreate.Count} day(s)."));
                }).WithName("CreateAbsenceForChild")
            .WithOpenApi()
            .RequireAuthorization(SystemRoles.ParentPolicy);

        return builder;
    }
}

public record CreateAbsenceRequest(DateTime StartDate, DateTime EndDate, int AbsenceStatusId);

public record CreateAbsenceResponse(string Message);