using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string Street { get; private set; } = string.Empty;
        public string? Suite { get; private set; }
        public string City { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty;
        public string PostalCode { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;

        private Address() { }

        public static Address Create(
            string street,
            string? suite,
            string city,
            string state,
            string postalCode,
            string country = "Türkiye")
        {
            return new Address
            {
                Street = street,
                Suite = suite,
                City = city,
                State = state,
                PostalCode = postalCode,
                Country = country
            };
        }

        public string GetFullAddress()
        {
            var addressParts = new List<string> { Street };
            if (!string.IsNullOrWhiteSpace(Suite))
                addressParts.Add(Suite);
            addressParts.Add($"{City}, {State} {PostalCode}");
            if (Country != "Türkiye")
                addressParts.Add(Country);

            return string.Join(", ", addressParts);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return Suite ?? string.Empty;
            yield return City;
            yield return State;
            yield return PostalCode;
            yield return Country;
        }
    }
}
