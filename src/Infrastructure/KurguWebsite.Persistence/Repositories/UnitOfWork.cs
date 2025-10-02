using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KurguWebsiteDbContext _context;
        private readonly ILogger<UnitOfWork>? _logger;
        private IDbContextTransaction? _currentTransaction;
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
        private IRefreshTokenRepository? _refreshTokens;
        private IServiceFeatureRepository? _serviceFeatures;
        private ICaseStudyMetricRepository? _caseStudyMetrics;
        private IAuditLogRepository? _auditLogs;

        public UnitOfWork(KurguWebsiteDbContext context, ILogger<UnitOfWork>? logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        // Repository Properties - Lazy initialization
        public IServiceRepository Services =>
            _services ??= new ServiceRepository(_context);

        public ICaseStudyRepository CaseStudies =>
            _caseStudies ??= new CaseStudyRepository(_context);

        public ITestimonialRepository Testimonials =>
            _testimonials ??= new TestimonialRepository(_context);

        public IPartnerRepository Partners =>
            _partners ??= new PartnerRepository(_context);

        public IPageRepository Pages =>
            _pages ??= new PageRepository(_context);

        public IContactMessageRepository ContactMessages =>
            _contactMessages ??= new ContactMessageRepository(_context);

        public ICompanyInfoRepository CompanyInfo =>
            _companyInfo ??= new CompanyInfoRepository(_context);

        public IProcessStepRepository ProcessSteps =>
            _processSteps ??= new ProcessStepRepository(_context);

        public IRefreshTokenRepository RefreshTokens =>
            _refreshTokens ??= new RefreshTokenRepository(_context);

        public IServiceFeatureRepository ServiceFeatures =>
            _serviceFeatures ??= new ServiceFeatureRepository(_context);

        public ICaseStudyMetricRepository CaseStudyMetrics =>
            _caseStudyMetrics ??= new CaseStudyMetricRepository(_context);

        public IAuditLogRepository AuditLogs =>
            _auditLogs ??= new AuditLogRepository(_context);

        // Check if there's an active transaction
        public bool HasActiveTransaction => _currentTransaction != null;

        // Save changes to database
        public async Task<int> CommitAsync()
        {
            return await CommitAsync(CancellationToken.None);
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                _logger?.LogInformation("Successfully saved {Count} entities to database", result);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Concurrency conflict occurred while saving changes");
                throw new InvalidOperationException(
                    "The entity you are trying to update has been modified by another user. Please reload and try again.",
                    ex);
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Database update exception occurred");
                throw new InvalidOperationException(
                    "An error occurred while saving changes to the database.",
                    ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error occurred while committing changes");
                throw;
            }
        }

        // Rollback pending changes (EF Core ChangeTracker level)
        public async Task RollbackAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entry.State = EntityState.Detached;
                                break;
                            case EntityState.Modified:
                            case EntityState.Deleted:
                                entry.Reload();
                                break;
                        }
                    }
                    _logger?.LogInformation("Successfully rolled back pending changes in ChangeTracker");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error occurred while rolling back changes");
                    throw;
                }
            });
        }

        // ==================== TRANSACTION MANAGEMENT ====================

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        public async Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                _logger?.LogWarning("Transaction already started. Returning existing transaction wrapper.");
                // Return a no-op disposable to avoid disposing the existing transaction
                return new NoOpDisposable();
            }

            try
            {
                _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger?.LogInformation(
                    "Transaction started with ID: {TransactionId}",
                    _currentTransaction.TransactionId);

                return new TransactionScope(this);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to begin transaction");
                throw;
            }
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            try
            {
                await _currentTransaction.CommitAsync(cancellationToken);
                _logger?.LogInformation(
                    "Transaction committed successfully with ID: {TransactionId}",
                    _currentTransaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error committing transaction. Rolling back...");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                _logger?.LogWarning("No active transaction to rollback.");
                return;
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                _logger?.LogInformation(
                    "Transaction rolled back with ID: {TransactionId}",
                    _currentTransaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error rolling back transaction");
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <summary>
        /// Disposes the current transaction
        /// </summary>
        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        // ==================== DISPOSE PATTERN ====================

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // If there's an active transaction, roll it back
                    if (_currentTransaction != null)
                    {
                        _logger?.LogWarning(
                            "UnitOfWork disposed with active transaction. Rolling back transaction {TransactionId}",
                            _currentTransaction.TransactionId);

                        try
                        {
                            _currentTransaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error rolling back transaction during disposal");
                        }
                        finally
                        {
                            _currentTransaction.Dispose();
                            _currentTransaction = null;
                        }
                    }

                    // Dispose context
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ==================== HELPER CLASSES ====================

        /// <summary>
        /// Helper class that provides a scope for transactions
        /// Automatically rolls back if not committed
        /// </summary>
        private class TransactionScope : IDisposable
        {
            private readonly UnitOfWork _unitOfWork;
            private bool _disposed;
            private bool _completed;

            public TransactionScope(UnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            public void Complete()
            {
                _completed = true;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (!_completed && _unitOfWork._currentTransaction != null)
                    {
                        _unitOfWork._logger?.LogWarning(
                            "TransactionScope disposed without commit. Rolling back transaction.");

                        try
                        {
                            // Synchronous rollback on dispose
                            _unitOfWork._currentTransaction.Rollback();
                            _unitOfWork._currentTransaction.Dispose();
                            _unitOfWork._currentTransaction = null;
                        }
                        catch (Exception ex)
                        {
                            _unitOfWork._logger?.LogError(
                                ex,
                                "Error rolling back transaction in TransactionScope disposal");
                        }
                    }
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// No-operation disposable for when a transaction already exists
        /// </summary>
        private class NoOpDisposable : IDisposable
        {
            public void Dispose()
            {
                // Intentionally empty - does nothing
            }
        }
    }
}