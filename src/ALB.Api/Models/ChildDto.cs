using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Entities;

namespace ALB.Api.Models;

public record ChildDto(string FirstName, string LastName, long DateOfBirth, Guid Id);

public static class ChildDtoExtensions
{
    public static ChildDto toDto(this Child child) => new(
        child.FirstName,
        child.LastName,
        child.DateOfBirth.ToUnixTimestamp(),
        child.Id
    );
}