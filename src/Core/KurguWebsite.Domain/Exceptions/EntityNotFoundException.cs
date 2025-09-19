using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Exceptions
{
    public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityName, Guid id)
            : base($"{entityName} with id {id} was not found") { }

        public EntityNotFoundException(string entityName, string identifier)
            : base($"{entityName} with identifier '{identifier}' was not found") { }
    }
}
