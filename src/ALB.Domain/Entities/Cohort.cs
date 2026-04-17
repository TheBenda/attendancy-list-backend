using System.Text.Json.Serialization;

namespace ALB.Domain.Entities;

public class Cohort
{
    public Guid Id { get; set; }
    public int CreationYear { get; set; }
    public Guid GroupId { get; set; }
    public Guid GradeId { get; set; }

    public Grade Grade { get; set; } = null!;
    public Group Group { get; set; } = null!;
    public ICollection<AttendanceList> AttendanceLists { get; set; } = new List<AttendanceList>();
}