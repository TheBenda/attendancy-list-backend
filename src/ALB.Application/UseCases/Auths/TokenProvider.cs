using System.Security.Claims;
using System.Text;

using ALB.Domain.Identity;
using ALB.Domain.Options;
using ALB.Domain.Repositories;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ALB.Application.UseCases.Auths;

public sealed class TokenProvider(UserManager<ApplicationUser> userManager, IRefreshTokenRepository refreshTokenRepository, IOptions<JwtOptions> options)
{
    public async Task<string> Create(ApplicationUser user, IList<string> roles)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.Value.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        var rokenDesc = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(options.Value.ExpirationInMinutes)),
            SigningCredentials = credentials,
            IssuedAt = DateTime.UtcNow,
            Issuer = options.Value.Issuer,
            Audience = options.Value.Audience,
        };

        var tokenHandler = new JsonWebTokenHandler();

        return tokenHandler.CreateToken(rokenDesc);
    }

    public async Task<string> GenerateRefreshToken(ApplicationUser user, CancellationToken ct)
    {
        return await refreshTokenRepository.CreateAsync(user, options.Value.ExpirationInMinutes * 5, ct);
    }

    public async Task<string> UpdateTokenExpiration(Guid refreshTokenId, CancellationToken ct)
    {
        return await refreshTokenRepository.UpdateExpiresOnUtcAsync(refreshTokenId, options.Value.ExpirationInMinutes * 5, ct);
    }
}