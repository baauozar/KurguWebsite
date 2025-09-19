using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Exceptions
{
    public class InvalidSlugException : DomainException
    {
        public InvalidSlugException(string slug)
            : base($"The slug '{slug}' is invalid or already exists") { }
    }
}
