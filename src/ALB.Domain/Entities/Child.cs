using System.Text.Json.Serialization;

using ALB.Domain.Identity;

using NodaTime;

namespace ALB.Domain.Entities;

public class Child
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public LocalDate DateOfBirth { get; set; }
    public Guid? CurrentGroupId { get; set; }
    public ICollection<ApplicationUser> Guardians { get; set; } = new List<ApplicationUser>();

    public Group? CurrentGroup { get; set; } = null!;

    public ICollection<AttendanceListEntry> AttendanceListEntries { get; set; } = new List<AttendanceListEntry>();

    public ICollection<AbsenceDay> AbsenceDays { get; set; } = new List<AbsenceDay>();
}