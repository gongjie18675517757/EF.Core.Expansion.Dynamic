using System;
using System.Collections.Generic;
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
        public static IQueryable<T> DynamicQuery<T>(this IQueryable<T> queryable, QueryCondition query)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            if (query == null)
            {
                return queryable;
            }

            return ExpressionExpand<T>.DynamicQuery(queryable, query);
        }

        /// <summary>
        /// 动态排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="sortings"></param>
        /// <returns></returns>
        public static IQueryable<T> DynamicSort<T>(this IQueryable<T> queryable, IEnumerable<SortingParameter> sortings)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            if (sortings == null || !sortings.Any())
            {
                return queryable;
            }


            return ExpressionExpand<T>.DynamicSort(queryable, sortings);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageQueryParameter"></param>
        /// <returns></returns>

        public static IEnumerable<T> PageQuery<T>(this IQueryable<T> @this, IPageQueryParameter pageQueryParameter)
        {
            @this = @this.DynamicQuery(pageQueryParameter.Condition);
            pageQueryParameter.Total = @this.Count();
            return @this.DynamicSort(pageQueryParameter.Sortings).ToList();
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

        /// <summary>
        /// 获取条件表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
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

    public interface IPageQueryParameter
    {
        /// <summary>
        /// 总数
        /// </summary>
        int Total { set; }

        /// <summary>
        /// 条件
        /// </summary>
        QueryCondition Condition { get; }

        /// <summary>
        /// 排序
        /// </summary>
        IEnumerable<SortingParameter> Sortings { get; }
    }
}
