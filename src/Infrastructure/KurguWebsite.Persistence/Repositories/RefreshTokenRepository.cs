using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetActiveTokenAsync(string token)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked && t.Expires > DateTime.UtcNow);
        }

        public async Task<IReadOnlyList<RefreshToken>> GetUserActiveTokensAsync(Guid userId)
        {
            return await _dbSet
                .Where(t => t.UserId == userId && !t.IsRevoked && t.Expires > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp)
        {
            var activeTokens = await _dbSet
                .Where(t => t.UserId == userId && !t.IsRevoked && t.Expires > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.Revoke(revokedByIp);
            }

            await _context.SaveChangesAsync();
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _dbSet
                .Where(t => t.Expires < DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            _dbSet.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}

