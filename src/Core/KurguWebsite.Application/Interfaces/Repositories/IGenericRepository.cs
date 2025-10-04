using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Specifications;
using System.Linq.Expressions;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
 /*       IQueryable<T> EntitiesIncludingDeleted { get; }
        IQueryable<T> Entities { get; }*/
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);

        // ===========================================
        // GET ALL
        // ===========================================
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

        // ===========================================
        // GET WITH PREDICATE
        // ===========================================
        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeString = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default);

        // ===========================================
        // FIRST OR DEFAULT / SINGLE OR DEFAULT
        // ===========================================
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<T?> FirstOrDefaultAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default);

        Task<T?> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        // ===========================================
        // SPECIFICATION PATTERN
        // ===========================================
        Task<T?> GetBySpecAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> ListAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default);

        // ===========================================
        // ADD / UPDATE / DELETE
        // ===========================================
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // ===========================================
        // SOFT DELETE / RESTORE
        // ===========================================
        Task RestoreAsync(T entity, CancellationToken cancellationToken = default);
        Task RestoreByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task RestoreRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // ===========================================
        // COUNT / EXISTS / ANY
        // ===========================================
        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default);
    }
}
