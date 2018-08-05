using System;
using System.Linq;
using System.Linq.Expressions;

namespace EF.Core.Expansion.Dynamic
{
    public static class Expand
    {
        /// <summary>
        /// 动态查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> AssemblyCondition<T>(this IQueryable<T> queryable, Query query)
        {
            return ExpressionExpand<T>.AssemblyCondition(queryable, query);
        }

        /// <summary>
        /// 获取条件表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetExpression<T>()
        {
            return ExpressionExpand<T>.GetExpression();
        }

        public static Expression<Func<T, bool>> GetExpression<T>(Expression<Func<T, bool>> expression)
        {
            return expression;
        }

        /// <summary>
        /// 条件：与
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> expression)
        {
            return ExpressionExpand<T>.And(@this, expression);
        }

        /// <summary>
        /// 条件：或
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> expression)
        {
            return ExpressionExpand<T>.Or(@this, expression);
        }
    }
}
