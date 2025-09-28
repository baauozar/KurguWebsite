using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface ICaseStudyUniquenessChecker
    {
        Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    }

}
