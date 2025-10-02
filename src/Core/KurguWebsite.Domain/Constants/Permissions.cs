using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Constants
{
    public static class Permissions
    {
        public static class Services
        {
            public const string View = "Permissions.Services.View";
            public const string Create = "Permissions.Services.Create";
            public const string Edit = "Permissions.Services.Edit";
            public const string Delete = "Permissions.Services.Delete";
            public const string Restore = "Permissions.Services.Restore";
        }

        public static class CaseStudies
        {
            public const string View = "Permissions.CaseStudies.View";
            public const string Create = "Permissions.CaseStudies.Create";
            public const string Edit = "Permissions.CaseStudies.Edit";
            public const string Delete = "Permissions.CaseStudies.Delete";
            public const string Restore = "Permissions.CaseStudies.Restore";
        }

        public static class Testimonials
        {
            public const string View = "Permissions.Testimonials.View";
            public const string Create = "Permissions.Testimonials.Create";
            public const string Edit = "Permissions.Testimonials.Edit";
            public const string Delete = "Permissions.Testimonials.Delete";
            public const string Restore = "Permissions.Testimonials.Restore";
        }

        public static class Partners
        {
            public const string View = "Permissions.Partners.View";
            public const string Create = "Permissions.Partners.Create";
            public const string Edit = "Permissions.Partners.Edit";
            public const string Delete = "Permissions.Partners.Delete";
        }

        public static class Pages
        {
            public const string View = "Permissions.Pages.View";
            public const string Edit = "Permissions.Pages.Edit";
        }

        public static class ProcessSteps
        {
            public const string View = "Permissions.ProcessSteps.View";
            public const string Create = "Permissions.ProcessSteps.Create";
            public const string Edit = "Permissions.ProcessSteps.Edit";
            public const string Delete = "Permissions.ProcessSteps.Delete";
        }

        public static class CompanyInfo
        {
            public const string View = "Permissions.CompanyInfo.View";
            public const string Edit = "Permissions.CompanyInfo.Edit";
        }

        public static class ContactMessages
        {
            public const string View = "Permissions.ContactMessages.View";
            public const string Delete = "Permissions.ContactMessages.Delete";
            public const string Reply = "Permissions.ContactMessages.Reply";
        }

        public static class AuditLogs
        {
            public const string View = "Permissions.AuditLogs.View";
            public const string Export = "Permissions.AuditLogs.Export";
        }

        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
            public const string ManageRoles = "Permissions.Users.ManageRoles";
            public const string ManagePermissions = "Permissions.Users.ManagePermissions";
        }

        public static class Dashboard
        {
            public const string View = "Permissions.Dashboard.View";
        }

        // Helper method to get all permissions
        public static List<string> GetAllPermissions()
        {
            var permissions = new List<string>();

            var permissionClasses = typeof(Permissions).GetNestedTypes();
            foreach (var permissionClass in permissionClasses)
            {
                var fields = permissionClass.GetFields();
                permissions.AddRange(fields.Select(f => f.GetValue(null)?.ToString() ?? string.Empty));
            }

            return permissions.Where(p => !string.IsNullOrEmpty(p)).ToList();
        }

        // Helper method to get permissions by module
        public static Dictionary<string, List<string>> GetPermissionsByModule()
        {
            var result = new Dictionary<string, List<string>>();

            var permissionClasses = typeof(Permissions).GetNestedTypes();
            foreach (var permissionClass in permissionClasses)
            {
                var moduleName = permissionClass.Name;
                var fields = permissionClass.GetFields();
                var modulePermissions = fields
                    .Select(f => f.GetValue(null)?.ToString() ?? string.Empty)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                result[moduleName] = modulePermissions;
            }

            return result;
        }
    }
}