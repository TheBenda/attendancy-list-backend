using ALB.Api.Models;
using ALB.Domain.Identity;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Users.Mappers;

internal static class UserMappingExtensions
{
    internal static GetUsersResponse ToResponse(this List<UserDto> users)
    {
        return new GetUsersResponse(users);
    }

    internal static UserDto ToDto(this ApplicationUser user, IList<string> userRoles) => new(user.Id, user.Email,
        user?.FirstName, user?.LastName, user?.PhoneNumber,
        userRoles.Select(r => r.ToDto()).ToHashSet() ?? []);

    private static UserRole ToDto(this string role)
    {
        return role switch
        {
            SystemRoles.Admin => UserRole.Admin,
            SystemRoles.CoAdmin => UserRole.CoAdmin,
            SystemRoles.Team => UserRole.Team,
            SystemRoles.Parent => UserRole.Parent,
            _ => throw new ArgumentOutOfRangeException(nameof(role), $"Not expected role value: {role}"),
        };
    }
}