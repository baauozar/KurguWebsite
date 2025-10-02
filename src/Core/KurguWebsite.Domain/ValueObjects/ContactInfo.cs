using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.ValueObjects
{
    public class ContactInfo : ValueObject
    {
        public string SupportPhone { get; private set; } = string.Empty;
        public string SalesPhone { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? SupportEmail { get; private set; }
        public string? SalesEmail { get; private set; }

        private ContactInfo() { }

        public static ContactInfo Create(
               string supportPhone,
               string salesPhone,
               string email,
               string? supportEmail = null,
               string? salesEmail = null)
        {
            return new ContactInfo
            {
                SupportPhone = supportPhone,
                SalesPhone = salesPhone,
                Email = email,
                SupportEmail = supportEmail ?? $"support@{GetDomain(email)}",
                SalesEmail = salesEmail ?? $"sales@{GetDomain(email)}"
            };
        }
        private static string GetDomain(string email)
        {
            var parts = email.Split('@');
            return parts.Length > 1 ? parts[1] : "domain.com";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SupportPhone;
            yield return SalesPhone;
            yield return Email;
        }
    }
}