// src/Core/KurguWebsite.Domain/Specifications/RefreshTokenSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    /// <summary>
    /// Get active refresh tokens for a user
    /// </summary>
    public class ActiveRefreshTokensByUserSpecification : BaseSpecification<RefreshToken>
    {
        public ActiveRefreshTokensByUserSpecification(Guid userId)
            : base(rt => rt.UserId == userId && !rt.IsRevoked && rt.Expires > DateTime.UtcNow)
        {
            ApplyOrderByDescending(rt => rt.Created);
        }
    }

    /// <summary>
    /// Get refresh token by token string
    /// </summary>
    public class RefreshTokenByTokenSpecification : BaseSpecification<RefreshToken>
    {
        public RefreshTokenByTokenSpecification(string token)
            : base(rt => rt.Token == token)
        {
        }
    }

    /// <summary>
    /// Get expired tokens for cleanup
    /// </summary>
    public class ExpiredRefreshTokensSpecification : BaseSpecification<RefreshToken>
    {
        public ExpiredRefreshTokensSpecification(int daysOld = 30)
            : base(rt => rt.Expires < DateTime.UtcNow.AddDays(-daysOld))
        {
            ApplyOrderBy(rt => rt.Expires);
        }
    }

    /// <summary>
    /// Get revoked tokens
    /// </summary>
    public class RevokedRefreshTokensSpecification : BaseSpecification<RefreshToken>
    {
        public RevokedRefreshTokensSpecification(Guid? userId = null)
            : base(rt => rt.IsRevoked && (userId == null || rt.UserId == userId))
        {
            ApplyOrderByDescending(rt => rt.Revoked);
        }
    }

    /// <summary>
    /// Get tokens created from specific IP
    /// </summary>
    public class RefreshTokensByIpSpecification : BaseSpecification<RefreshToken>
    {
        public RefreshTokensByIpSpecification(string ipAddress)
            : base(rt => rt.CreatedByIp == ipAddress)
        {
            ApplyOrderByDescending(rt => rt.Created);
        }
    }
}