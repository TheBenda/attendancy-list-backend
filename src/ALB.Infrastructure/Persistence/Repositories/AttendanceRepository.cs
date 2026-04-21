using ALB.Domain.Entities;
using ALB.Domain.Enum;
using ALB.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

using NodaTime;

namespace ALB.Infrastructure.Persistence.Repositories;

public class AttendanceRepository(ApplicationDbContext dbContext) : IAttendanceRepository
{
    public async Task CreateAsync(Guid childId, LocalDate date, LocalTime? arrivalAt, LocalTime? departureAt,
        AttendanceStatus status, CancellationToken ct)
    {
        var attendance = await dbContext.AttendanceListEntries
            .FirstOrDefaultAsync(a => a.ChildId == childId && a.Date == date, ct);

        if (attendance is null)
        {
            attendance = new AttendanceListEntry
            {
                Id = Guid.NewGuid(),
                ChildId = childId,
                Date = date,
                ArrivalAt = arrivalAt,
                DepartureAt = departureAt,
                AttendanceStatus = status
            };

            dbContext.AttendanceListEntries.Add(attendance);

            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateAsync(Guid childId, LocalDate date, LocalTime? arrivalAt, LocalTime? departureAt,
        AttendanceStatus status, CancellationToken ct)
    {
        var attendance = await dbContext.AttendanceListEntries
            .FirstOrDefaultAsync(a => a.ChildId == childId && a.Date == date, ct);

        if (attendance is not null)
        {
            attendance.ArrivalAt = arrivalAt;
            attendance.DepartureAt = departureAt;
            attendance.AttendanceStatus = status;
            dbContext.AttendanceListEntries.Update(attendance);

            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAsync(Guid childId, LocalDate date, CancellationToken ct)
    {
        var attendance = await dbContext.AttendanceListEntries
            .FirstOrDefaultAsync(a => a.ChildId == childId && a.Date == date, ct);

        if (attendance is null)
            throw new Exception("Attendance not found");

        dbContext.AttendanceListEntries.Remove(attendance);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<AttendanceList?> GetAttendanceListByIdAsync(Guid id)
    {
        return await dbContext.AttendanceLists
            .FirstOrDefaultAsync(al => al.Id == id);
    }
}