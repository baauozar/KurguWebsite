using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Specifications;
using KurguWebsite.Persistence.Context;
using KurguWebsite.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KurguWebsite.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly KurguWebsiteDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(KurguWebsiteDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // ===========================================
        // QUERYABLE PROPERTIES
        // ===========================================
     /*   public IQueryable<T> Entities => _dbSet;
        public IQueryable<T> EntitiesIncludingDeleted => _dbSet.IgnoreQueryFilters();
*/
        // ===========================================
        // GET BY ID
        // ===========================================
        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }
   

        public virtual async Task<T?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        // ===========================================
        // GET ALL
        // ===========================================
        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(cancellationToken);
        }

        // ===========================================
        // GET WITH PREDICATE
        // ===========================================
        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeString = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(includeString))
                query = query.Include(includeString);

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync(cancellationToken);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync(cancellationToken);

            return await query.ToListAsync(cancellationToken);
        }

        // ===========================================
        // FIRST OR DEFAULT / SINGLE OR DEFAULT
        // ===========================================
        public virtual async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<T?> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
        }

        // ===========================================
        // SPECIFICATION PATTERN
        // ===========================================
        public async Task<T?> GetBySpecAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.ToListAsync(cancellationToken);
        }

        // ===========================================
        // ADD / UPDATE / DELETE
        // ===========================================
        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public virtual Task UpdateRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is AuditableEntity auditable)
            {
                // Soft delete
                auditable.SoftDelete("system"); // TODO: Inject ICurrentUserService to get actual user
                _dbSet.Update(entity);
            }
            else
            {
                // Hard delete
                _dbSet.Remove(entity);
            }
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                if (entity is AuditableEntity auditable)
                {
                    auditable.SoftDelete("system");
                    _dbSet.Update(entity);
                }
                else
                {
                    _dbSet.Remove(entity);
                }
            }
            return Task.CompletedTask;
        }

        // ===========================================
        // SOFT DELETE / RESTORE
        // ===========================================
        public virtual Task RestoreAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is AuditableEntity auditable)
            {
                auditable.Restore();
                _dbSet.Update(entity);
            }
            return Task.CompletedTask;
        }

        public virtual async Task RestoreByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null) return;

            if (entity is AuditableEntity auditable)
            {
                auditable.Restore();
                _dbSet.Update(entity);
            }
        }

        public virtual Task RestoreRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                if (entity is AuditableEntity auditable)
                {
                    auditable.Restore();
                    _dbSet.Update(entity);
                }
            }
            return Task.CompletedTask;
        }

        // ===========================================
        // COUNT / EXISTS / ANY
        // ===========================================
        public virtual async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                return await _dbSet.CountAsync(cancellationToken);

            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.CountAsync(cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.AnyAsync(cancellationToken);
        }

        // ===========================================
        // PAGINATION
        // ===========================================
        public virtual async Task<PaginatedList<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            var count = await query.CountAsync(cancellationToken);

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }

        public virtual async Task<PaginatedList<T>> GetPagedAsync(
            ISpecification<T> spec,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);

            var count = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }

        // ===========================================
        // HELPER METHODS
        // ===========================================
        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
        }
    }
}