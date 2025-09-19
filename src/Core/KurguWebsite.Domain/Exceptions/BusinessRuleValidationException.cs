using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Exceptions
{
    public class BusinessRuleValidationException : DomainException
    {
        public string Details { get; }

        public BusinessRuleValidationException(string message, string details = "")
            : base(message)
        {
            Details = details;
        }
    }
}
