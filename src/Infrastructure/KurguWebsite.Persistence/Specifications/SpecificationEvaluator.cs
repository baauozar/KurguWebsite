// src/Infrastructure/KurguWebsite.Persistence/Specifications/SpecificationEvaluator.cs
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KurguWebsite.Persistence.Specifications
{
    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            // Apply filtering
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Apply includes using expressions
            query = specification.Includes.Aggregate(query,
                (current, include) => current.Include(include));

            // Apply string-based includes (for nested includes)
            query = specification.IncludeStrings.Aggregate(query,
                (current, include) => current.Include(include));

            // Apply ordering
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Apply paging
            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip)
                             .Take(specification.Take);
            }

            return query;
        }

        public static IQueryable<TEntity> GetQueryWithoutPaging(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            // Apply filtering
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Apply includes
            query = specification.Includes.Aggregate(query,
                (current, include) => current.Include(include));

            query = specification.IncludeStrings.Aggregate(query,
                (current, include) => current.Include(include));

            // Apply ordering
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Don't apply paging for count operations

            return query;
        }
    }
}
