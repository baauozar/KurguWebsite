using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class RefreshToken: BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public string? ReplacedByToken { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }

        public bool IsActive => !IsRevoked && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= Expires;
        private RefreshToken() { }

        public static RefreshToken Create(
            Guid userId,
            string token,
            string createdByIp,
            int daysToExpire = 7)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = token,
                CreatedByIp = createdByIp,
                Expires = DateTime.UtcNow.AddDays(daysToExpire)
            };
        }

        public void Revoke(string revokedByIp, string? replacedByToken = null)
        {
            IsRevoked = true;
            Revoked = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            ReplacedByToken = replacedByToken;
        }
    }
}