namespace KurguWebsite.Application.Common.Interfaces.Repositories
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
