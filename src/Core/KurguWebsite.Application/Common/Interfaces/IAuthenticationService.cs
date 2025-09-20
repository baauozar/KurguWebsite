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
        Task<bool> LogoutAsync(); // current user
        Task<bool> UserExistsAsync(string email);
        Task<AuthenticationResultModel> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword); // current user
        Task<bool> ResetPasswordAsync(string email); // maybe admin reset other user passwords
        Task<bool> ConfirmEmailAsync(string token); // current user
    }


}
