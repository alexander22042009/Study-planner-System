using StudyPlanner.Core.Entities;

namespace StudyPlanner.Core.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);

    string GenerateRefreshToken();

    DateTime GetAccessTokenExpiration();

    Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    Task RevokeAllUserTokensAsync(string userId, CancellationToken cancellationToken = default);
}
