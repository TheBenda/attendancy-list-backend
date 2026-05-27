using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ALB.Api.Endpoints.Mappers;
using ALB.Domain.Identity;
using ALB.Domain.Options;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ALB.Api.Endpoints.Users;

internal static class RegisterInvitedUserEndpoint
{
    internal static IEndpointRouteBuilder MapRegisterInvitedUserEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/register-invited-user", async (ClaimsPrincipal principal, RegisterInvitedUserRequest request, UserManager<ApplicationUser> userManager, IInviteUsersRepository inviteUsersRepository, IOptions<JwtOptions> options) =>
            {
                var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email))
                    return Results.BadRequest("Invalid email claim.");

                if (!Guid.TryParse(sub, out var inviteUserId))
                    return Results.BadRequest("Invalid subject claim.");
                
                var foundUser = await inviteUsersRepository.GetByIdAsync(inviteUserId);

                if (foundUser is null)
                {
                    return Results.BadRequest("Invalid token.");
                }
                
                var user = new ApplicationUser
                {
                    Email = foundUser.Email,
                    UserName = foundUser.Email,
                    FirstName = foundUser.FirstNames,
                    LastName = foundUser.LastNames,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return Results.BadRequest(result.Errors.AsErrorString());
                }
                
                inviteUsersRepository.DeleteAsync(foundUser);

                return Results.Ok(new CreateUserResponse(user.Id, user.Email, user.FirstName, user.LastName));
            }).WithName("RegisterInvitedUser")
            .Produces<CreateUserResponse>()
            .ProducesProblem(400)
            .RequireAuthorization(SystemRoles.InvitedPolicy);
        
        return routeBuilder;
    }
    
    private static ClaimsPrincipal ValidateInviteToken(this string token, JwtOptions jwtOptions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)),

            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        return tokenHandler.ValidateToken(token, validationParameters, out _);
    }
}

internal record RegisterInvitedUserRequest(string Password);