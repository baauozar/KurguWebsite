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
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}
