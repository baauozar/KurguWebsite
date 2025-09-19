using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class Client: BaseEntity
    {
        public string? Name { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? Testimonial { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactTitle { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

    }
}
