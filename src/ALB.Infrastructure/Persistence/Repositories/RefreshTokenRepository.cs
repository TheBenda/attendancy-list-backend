using System.Security.Cryptography;

using ALB.Domain.Identity;
using ALB.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ALB.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    public async Task<string> CreateAsync(ApplicationUser user, int expiresInMinutes, CancellationToken cancellationToken = default)
    {
        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var newToken = new RefreshToken
        {
            UserId = user.Id,
            Value = tokenValue,
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(expiresInMinutes))
        };

        await context.RefreshTokens.AddAsync(newToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return tokenValue;
    }

    public async Task<RefreshToken?> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => await context.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(rt => rt.Value == refreshToken, cancellationToken);

    public async Task<string> UpdateExpiresOnUtcAsync(Guid refreshTokenId, int expiresInMinutes, CancellationToken cancellationToken = default)
    {
        var newTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        await context.RefreshTokens
            .Where(t => t.Id == refreshTokenId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.ExpiresOnUtc, DateTime.UtcNow.AddMinutes(Convert.ToDouble(expiresInMinutes)))
                .SetProperty(p => p.Value, newTokenValue),
                cancellationToken);

        return newTokenValue;
    }

    public Task DeleteAsync(string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}