
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
        public IQueryable<T> Entities => _context.Set<T>();

        public IQueryable<T> EntitiesIncludingDeleted => _dbSet.IgnoreQueryFilters();

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeString = null,
            bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(includeString))
                query = query.Include(includeString);

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync();

            return await query.ToListAsync();
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync();

            return await query.ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            if (entity is AuditableEntity aud)
            {
                // if you have ICurrentUserService, inject and use its UserId here
                aud.SoftDelete("system");
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
            return Task.CompletedTask;
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        
        public Task RestoreAsync(T entity)
        {
            if (entity is AuditableEntity auditable)
            {
                auditable.Restore();
                _dbSet.Update(entity);
            }
            // If not AuditableEntity, nothing to restore.
            return Task.CompletedTask;
        }

        public async Task RestoreByIdAsync(Guid id)
        {
            var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return;

            if (entity is AuditableEntity auditable)
            {
                auditable.Restore();
                _dbSet.Update(entity);
            }
        }

        public async Task<T?> GetByIdIncludingDeletedAsync(Guid id)
         => await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id);

    
      public async Task<T?> GetBySpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.CountAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            return await query.AnyAsync(cancellationToken);
        }

        // Helper method to apply specification
        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
        }
    }
}
