using System.Text.Json.Serialization;

namespace ALB.Api.Models;

public record UserDto(
    Guid Id,
    string? Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    HashSet<UserRole> Roles
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Admin, CoAdmin, Team, Parent
}