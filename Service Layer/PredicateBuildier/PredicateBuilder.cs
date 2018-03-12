using DataAccessLayer.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Service_Layer.PredicateBuildier
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<Product, bool>> SearchByDetails(Dictionary<string, IEnumerable<string>> details)
        {
            var predicate = True<Product>();

            foreach (var detail in details)
            {
                var predicate2 = False<Product>();
                foreach (var value in detail.Value)
                {
                    predicate2 = predicate2.Or(p => p.ProductDetails.Any(d => d.DetailName == detail.Key && d.DetailValue == value));
                }
                predicate = predicate.And(predicate2.Expand());
            }

            return predicate.Expand();
        }
    }
}
