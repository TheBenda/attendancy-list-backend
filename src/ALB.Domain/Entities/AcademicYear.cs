namespace ALB.Domain.Entities;

public class AcademicYear
{
    public Guid Id { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}