using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Specifications.Builders
{
    public class ServiceSpecificationBuilder
    {
        private readonly List<Expression<Func<Service, bool>>> _criteria = new();
        private readonly List<Expression<Func<Service, object>>> _includes = new();
        private Expression<Func<Service, object>>? _orderBy;
        private Expression<Func<Service, object>>? _orderByDescending;
        private int? _skip;
        private int? _take;

        public ServiceSpecificationBuilder Active()
        {
            _criteria.Add(s => s.IsActive && !s.IsDeleted);
            return this;
        }

        public ServiceSpecificationBuilder Featured()
        {
            _criteria.Add(s => s.IsFeatured);
            return this;
        }

        public ServiceSpecificationBuilder InCategory(ServiceCategory category)
        {
            _criteria.Add(s => s.Category == category);
            return this;
        }

        public ServiceSpecificationBuilder WithSearchTerm(string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                _criteria.Add(s => s.Title.Contains(searchTerm) ||
                                  s.Description.Contains(searchTerm));
            }
            return this;
        }

        public ServiceSpecificationBuilder IncludeFeatures()
        {
            _includes.Add(s => s.Features);
            return this;
        }

        public ServiceSpecificationBuilder IncludeCaseStudies()
        {
            _includes.Add(s => s.CaseStudies);
            return this;
        }

        public ServiceSpecificationBuilder OrderBy(Expression<Func<Service, object>> orderBy)
        {
            _orderBy = orderBy;
            return this;
        }

        public ServiceSpecificationBuilder OrderByDescending(Expression<Func<Service, object>> orderByDesc)
        {
            _orderByDescending = orderByDesc;
            return this;
        }

        public ServiceSpecificationBuilder Paginate(int pageNumber, int pageSize)
        {
            _skip = (pageNumber - 1) * pageSize;
            _take = pageSize;
            return this;
        }

        public ISpecification<Service> Build()
        {
            return new DynamicSpecification<Service>(
                _criteria,
                _includes,
                _orderBy,
                _orderByDescending,
                _skip,
                _take);
        }
    }

    public class DynamicSpecification<T> : BaseSpecification<T>
    {
        public DynamicSpecification(
            List<Expression<Func<T, bool>>> criteria,
            List<Expression<Func<T, object>>> includes,
            Expression<Func<T, object>>? orderBy,
            Expression<Func<T, object>>? orderByDescending,
            int? skip,
            int? take)
            : base(CombineCriteria(criteria))
        {
            foreach (var include in includes)
            {
                AddInclude(include);
            }

            if (orderBy != null)
                ApplyOrderBy(orderBy);
            else if (orderByDescending != null)
                ApplyOrderByDescending(orderByDescending);

            if (skip.HasValue && take.HasValue)
                ApplyPaging(skip.Value, take.Value);
        }

        private static Expression<Func<T, bool>> CombineCriteria(List<Expression<Func<T, bool>>> criteria)
        {
            if (!criteria.Any())
                return x => true;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression body = Expression.Constant(true);

            foreach (var criterion in criteria)
            {
                var visitor = new ParameterReplacer(criterion.Parameters[0], parameter);
                var criterionBody = visitor.Visit(criterion.Body);
                body = Expression.AndAlso(body, criterionBody);
            }

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
    }
}