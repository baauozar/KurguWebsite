using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.ValueObjects
{
    public class SocialMediaLinks : ValueObject
    {
        public string? Facebook { get; private set; }
        public string? Twitter { get; private set; }
        public string? LinkedIn { get; private set; }
        public string? Instagram { get; private set; }
        public string? YouTube { get; private set; }

        private SocialMediaLinks() { }

        public static SocialMediaLinks Create(
            string? facebook = null,
            string? twitter = null,
            string? linkedIn = null,
            string? instagram = null,
            string? youTube = null)
        {
            return new SocialMediaLinks
            {
                Facebook = facebook,
                Twitter = twitter,
                LinkedIn = linkedIn,
                Instagram = instagram,
                YouTube = youTube
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Facebook ?? string.Empty;
            yield return Twitter ?? string.Empty;
            yield return LinkedIn ?? string.Empty;
            yield return Instagram ?? string.Empty;
            yield return YouTube ?? string.Empty;
        }
    }
}