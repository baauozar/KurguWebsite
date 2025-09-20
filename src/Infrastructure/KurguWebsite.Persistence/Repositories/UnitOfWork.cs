using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KurguWebsiteDbContext _context;
        private bool _disposed;

        // Repository instances
        private IServiceRepository? _services;
        private ICaseStudyRepository? _caseStudies;
        private ITestimonialRepository? _testimonials;
        private IPartnerRepository? _partners;
        private IPageRepository? _pages;
        private IContactMessageRepository? _contactMessages;
        private ICompanyInfoRepository? _companyInfo;
        private IProcessStepRepository? _processSteps;

        public UnitOfWork(KurguWebsiteDbContext context)
        {
            _context = context;
        }

        public IServiceRepository Services => _services ??= new ServiceRepository(_context);
        public ICaseStudyRepository CaseStudies => _caseStudies ??= new CaseStudyRepository(_context);
        public ITestimonialRepository Testimonials => _testimonials ??= new TestimonialRepository(_context);
        public IPartnerRepository Partners => _partners ??= new PartnerRepository(_context);
        public IPageRepository Pages => _pages ??= new PageRepository(_context);
        public IContactMessageRepository ContactMessages => _contactMessages ??= new ContactMessageRepository(_context);
        public ICompanyInfoRepository CompanyInfo => _companyInfo ??= new CompanyInfoRepository(_context);
        public IProcessStepRepository ProcessSteps => _processSteps ??= new ProcessStepRepository(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RollbackAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}