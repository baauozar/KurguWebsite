using KurguWebsite.Application.Common.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IServiceRepository Services { get; }
        ICaseStudyRepository CaseStudies { get; }
        ITestimonialRepository Testimonials { get; }
        IPartnerRepository Partners { get; }
        IPageRepository Pages { get; }
        IContactMessageRepository ContactMessages { get; }
        ICompanyInfoRepository CompanyInfo { get; }
        IProcessStepRepository ProcessSteps { get; }

        Task<int> CommitAsync();
        Task<int> CommitAsync(CancellationToken cancellationToken);
        Task RollbackAsync();
    }
}
