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
        Task<AuthenticationResultModel> LoginAsync(string email, string password);
        Task<AuthenticationResultModel> RegisterAsync(RegisterRequest request);
        Task<bool> LogoutAsync(Guid userId); // Guid here
        Task<bool> UserExistsAsync(string email);
        Task<AuthenticationResultModel> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword); // Guid
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ConfirmEmailAsync(Guid userId, string token); // Guid
    }

}
