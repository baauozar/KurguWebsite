using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class PasswordHistory : BaseEntity
    {
        public Guid UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime SetDate { get; set; }
    }
}
