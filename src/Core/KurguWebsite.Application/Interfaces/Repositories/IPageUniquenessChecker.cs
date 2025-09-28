using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface IPageUniquenessChecker
    {
        Task<bool> PageSlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    }
}
