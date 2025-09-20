using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public class AuthenticationResultModel
    {
        public bool Success { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}
