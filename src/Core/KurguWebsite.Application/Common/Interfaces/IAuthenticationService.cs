using KurguWebsite.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string email, string password);
        Task<AuthenticationResult> RegisterAsync(RegisterRequest request);
        Task<bool> LogoutAsync(string userId);
        Task<bool> UserExistsAsync(string email);
        Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ConfirmEmailAsync(string userId, string token);
    }
   
}
