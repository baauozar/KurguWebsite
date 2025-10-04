// src/Core/KurguWebsite.Domain/Specifications/CompositeSpecifications.cs
using KurguWebsite.Domain.Entities;
using System.Linq.Expressions;

namespace KurguWebsite.Domain.Specifications
{
    /// <summary>
    /// Combines multiple specifications with AND logic
    /// </summary>
    public class AndSpecification<T> : BaseSpecification<T>
    {
        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
            : base(CombineExpressions(left.Criteria, right.Criteria))
        {
            foreach (var include in left.Includes.Union(right.Includes))
            {
                AddInclude(include);
            }

            foreach (var includeString in left.IncludeStrings.Union(right.IncludeStrings))
            {
                AddInclude(includeString);
            }
        }

        private static Expression<Func<T, bool>> CombineExpressions(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
            var leftBody = leftVisitor.Visit(left.Body);
            var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
            var rightBody = rightVisitor.Visit(right.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(leftBody, rightBody), parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                return node == _oldValue ? _newValue : base.Visit(node);
            }
        }
    }

    /// <summary>
    /// Combines multiple specifications with OR logic
    /// </summary>
    public class OrSpecification<T> : BaseSpecification<T>
    {
        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
            : base(CombineExpressions(left.Criteria, right.Criteria))
        {
            foreach (var include in left.Includes.Union(right.Includes))
            {
                AddInclude(include);
            }
        }

        private static Expression<Func<T, bool>> CombineExpressions(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
            var leftBody = leftVisitor.Visit(left.Body);
            var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
            var rightBody = rightVisitor.Visit(right.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(leftBody, rightBody), parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                return node == _oldValue ? _newValue : base.Visit(node);
            }
        }
    }

    /// <summary>
    /// Negates a specification
    /// </summary>
    public class NotSpecification<T> : BaseSpecification<T>
    {
        public NotSpecification(ISpecification<T> specification)
            : base(NegateExpression(specification.Criteria))
        {
            foreach (var include in specification.Includes)
            {
                AddInclude(include);
            }
        }

        private static Expression<Func<T, bool>> NegateExpression(
            Expression<Func<T, bool>> expression)
        {
            var parameter = expression.Parameters[0];
            var negated = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(negated, parameter);
        }
    }
}