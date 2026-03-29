using ALB.Domain.Identity;

namespace ALB.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<string> CreateAsync(ApplicationUser user, int expiresInMinutes, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<string> UpdateExpiresOnUtcAsync(Guid refreshTokenId, int expiresInMinutes,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(string token, CancellationToken cancellationToken = default);
}