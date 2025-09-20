using KurguWebsite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetActiveTokenAsync(string token);
        Task<IReadOnlyList<RefreshToken>> GetUserActiveTokensAsync(Guid userId);
        Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp);
        Task CleanupExpiredTokensAsync();
    }
}
