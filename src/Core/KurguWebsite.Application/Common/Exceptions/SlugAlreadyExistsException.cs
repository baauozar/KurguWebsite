using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Exceptions
{
    public class SlugAlreadyExistsException : ApplicationException
    {
        public SlugAlreadyExistsException(string slug)
            : base($"The slug '{slug}' already exists. Please choose a different title.") { }
    }
}
