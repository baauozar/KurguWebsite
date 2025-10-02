using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<bool> GrantPermissionAsync(string userId, string permission);
        Task<bool> RevokePermissionAsync(string userId, string permission);
        Task<bool> GrantPermissionsToRoleAsync(string roleName, List<string> permissions);
        Task<bool> RevokePermissionsFromRoleAsync(string roleName, List<string> permissions);
        Task<List<string>> GetRolePermissionsAsync(string roleName);
    }
}
