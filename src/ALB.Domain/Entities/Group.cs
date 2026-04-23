using ALB.Domain.Identity;

namespace ALB.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid AcademicYearId { get; init; }
    public required AcademicYear AcademicYear { get; init; }
    public Guid GroupnameId { get; init; }
    public required AllowedGroupname Groupname { get; init; }
    public Guid ResponsibleUserId { get; set; }
    public ApplicationUser ResponsibleUser { get; set; } = null!;
    public ICollection<Child> Children { get; set; } = new List<Child>();
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}