using System.Text.Json.Serialization;

using NodaTime;

namespace ALB.Domain.Entities;

public class AttendanceList
{
    public Guid Id { get; set; }
    public bool Open { get; set; } = true;
    public Guid GroupId { get; set; }
    public required DateInterval ValidationPeriod { get; set; }

    public Group Group { get; set; } = null!;
    [JsonIgnore]
    public ICollection<AttendanceListEntry> AttendanceListEntries { get; set; } = new List<AttendanceListEntry>();
    [JsonIgnore]
    public ICollection<AttendanceListWriter> Writers { get; set; } = new List<AttendanceListWriter>();

}