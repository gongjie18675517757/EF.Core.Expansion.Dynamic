using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EF.Core.Expansion.Dynamic
{
    public class ExpressionExpand<T> : ExpressionExpand
    {
        /// <summary>
        /// 参数表达式
        /// </summary>
        public static ParameterExpression ParameterExpression { get; } = Expression.Parameter(typeof(T));

        /// <summary>
        /// 获取条件表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetExpression()
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), ParameterExpression);
        }

        /// <summary>
        /// 拼条件：与
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var expressions = left.Parameters.Cast<Expression>();
            var invocationExpression = Expression.Invoke(right, expressions);
            return Expression.Lambda<Func<T, bool>>(Expression.And(left.Body, invocationExpression), left.Parameters);
        }

        /// <summary>
        /// 拼条件：或
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var expressions = left.Parameters.Cast<Expression>();
            var invocationExpression = Expression.Invoke(right, expressions);

            return Expression.Lambda<Func<T, bool>>(Expression.Or(left.Body, invocationExpression), left.Parameters);
        }

        /// <summary>
        /// 动态排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="sortings"></param>
        /// <returns></returns>
        public static IQueryable<T> DynamicSort(IQueryable<T> queryable, IEnumerable<SortingParameter> sortings)
        {
            var index = 0;
            foreach (var item in sortings)
            {
                index++;
                var memberExpression = GetMemberExpression(item.Name, out var propertyInfo);

                var methodInfo = typeof(ExpressionExpand<T>)
                                 .GetMethod("GenerateDynamicSort", BindingFlags.Instance |
                                                          BindingFlags.Static |
                                                         //BindingFlags.Public |
                                                         BindingFlags.NonPublic |
                                                         BindingFlags.DeclaredOnly)
                                 .MakeGenericMethod(GetNullableType(propertyInfo.PropertyType));
                queryable = (IQueryable<T>)methodInfo.Invoke(null, new object[] { queryable, item, index });
            }

            return queryable;
        }

        /// <summary>
        /// 拼排序表达式
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="sorting"></param>
        /// <returns></returns>
        private static IQueryable<T> GenerateDynamicSort<TKey>(IQueryable<T> queryable, SortingParameter sorting, int sort)
        {
            var memberExpression = GetMemberExpression(sorting.Name, out var propertyInfo);
            var expression = Expression.Lambda<Func<T, TKey>>(memberExpression, ParameterExpression);
            switch (sorting.SortMark)
            {
                case SortMark.Dsc:
                    if (sort > 1 && queryable is IOrderedQueryable<T> orderQueryable)
                        queryable = orderQueryable.ThenBy(expression);
                    else
                        queryable = queryable.OrderBy(expression);
                    break;
                case SortMark.Desc:
                    if (sort > 1 && queryable is IOrderedQueryable<T> orderQueryable2)
                        queryable = orderQueryable2.ThenByDescending(expression);
                    else
                        queryable = queryable.OrderByDescending(expression);
                    break;
                default:
                    break;
            }

            return queryable;
        }



        /// <summary>
        /// 组装条件
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> DynamicQuery(IQueryable<T> queryable, QueryCondition query)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (query.Keyword == null && query.Filter == null)
                return queryable;

            /*拼模糊查询条件*/
            if (query.Keyword != null && query.Keyword.Keywords != null && query.Keyword.Keywords.Any())
            {
                var expressions = query.Keyword.Keywords.Select(x => GetKeywordExpression(x));
                var expression = MergeExpression(query.Keyword.MultipleMark, expressions);

                var condExpression = Expression.Lambda<Func<T, bool>>(expression, ParameterExpression);
                queryable = queryable.Where(condExpression);
            }
            if (query.Filter != null)
            {
                var condExpression = Expression.Lambda<Func<T, bool>>(GenerateFilterExpression(query.Filter), ParameterExpression);
                queryable = queryable.Where(condExpression);
            }

            return queryable;
        }

        /// <summary>
        /// 组装模糊查询表达式
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static Expression GetKeywordExpression(string keyword)
        {
            var constantExpression = Expression.Constant(keyword);
            var binaryExpressions = new List<Expression>();
            foreach (var item in typeof(T).GetProperties())
            {
                if (item.PropertyType == typeof(string) && !item.Name.Contains("Id"))
                {
                    var memberExpression = Expression.Property(ParameterExpression, item);

                    var methodCallExpression = Expression.Call(memberExpression, typeof(string).GetMethod("Contains"), constantExpression);

                    var binaryExpression = Expression.And(Expression.NotEqual(memberExpression, Expression.Constant(null)), methodCallExpression);

                    binaryExpressions.Add(binaryExpression);
                }
            }

            Expression expressionBody = null;

            if (binaryExpressions.Count > 0)
            {
                if (binaryExpressions.Count > 1)
                {
                    Expression bodyExpression = null;
                    for (int i = 0; i < binaryExpressions.Count; i++)
                    {
                        if (i == 0)
                            bodyExpression = binaryExpressions[i];
                        else
                            bodyExpression = Expression.Or(bodyExpression, binaryExpressions[i]);
                    }

                    expressionBody = bodyExpression;
                }
                else
                {
                    expressionBody = binaryExpressions[0];
                }
            }
            else
            {
                expressionBody = Expression.Constant(true);
            }

            return expressionBody;

            //var Ex = Expression.Lambda<Func<T, bool>>(expressionBody, parameterExpression);
            //return Ex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Expression GenerateFilterExpression(Filter filter)
        {
            var expressions = new List<Expression>();

            /*拼多值[比较?><==..]查询条件*/
            if (filter.CompareConditions != null && filter.CompareConditions.Any())
            {
                expressions.AddRange(filter.CompareConditions.Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Compare) && !string.IsNullOrWhiteSpace(x.Value))
                    .Select(x => GetOperaterExpression(x.Name, x.Compare, x.Value)));
            }

            /*拼多值[匹配,in not]查询条件*/
            if (filter.MuchConditions != null && filter.MuchConditions.Any())
            {
                expressions.AddRange(filter.MuchConditions.Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Compare) && x.Values != null && x.Values.Any())
                    .Select(x => GetMatchExpression(x.Name, x.Compare, x.Values)));
            }

            /*合并*/
            var expression = MergeExpression(filter.MultipleMark, expressions);

            /*拼子条件*/
            if (filter.Filters != null && filter.Filters.Any())
            {
                var expressions2 = filter.Filters.Select(x => GenerateFilterExpression(x));
                var expressions3 = new[] { expression }.Union(expressions2);
                expression = MergeExpression(filter.MultipleMark, expressions3);
            }

            return expression;
        }



        public static MemberExpression GetMemberExpression(string name, out PropertyInfo propertyInfo)
        {
            return GetMemberExpression(typeof(T), name, out propertyInfo);
        }

        public static MemberExpression GetMemberExpression(Type type, string name, out PropertyInfo propertyinfo)
        {
            var propertyDic = GetPropertyDic(type);

            /*匹配a.b.c多级属性*/
            var names = name.ToLower().Split(".".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            name = names[0];

            if (!propertyDic.ContainsKey(name))
                throw new Exception($"{name}属性不存在于类型{type.Name}");

            propertyinfo = propertyDic[name];

            var memberExpression = Expression.Property(ParameterExpression, propertyinfo);


            for (int i = 1; i < names.Length; i++)
            {
                name = names[i];
                propertyDic = GetPropertyDic(propertyinfo.PropertyType);
                if (!propertyDic.ContainsKey(name))
                    throw new Exception($"{name}属性不存在于类型{type.Name}");
                propertyinfo = propertyDic[name];
                memberExpression = Expression.Property(memberExpression, propertyinfo);
            }

            return memberExpression;
        }

        /// <summary>
        /// 创建比较表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="compare"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression GetOperaterExpression(string name, string compare, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (compare == null)
            {
                throw new ArgumentNullException(nameof(compare));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            /*属性表达式*/
            var memberExpression = GetMemberExpression(name, out var propertyInfo);

            if (!ConvertType(propertyInfo.PropertyType, value, out var result))
                throw new Exception($"值:{value}无法转换成{propertyInfo.PropertyType.Name}类型");
            /*值表达式*/
            Expression constantExpression = Expression.Constant(result);

            switch (compare.ToLower())
            {
                case "==":
                    return Expression.Equal(memberExpression, constantExpression);
                case "!=":
                    return Expression.NotEqual(memberExpression, constantExpression);
                case "contains":
                    return Expression.And(Expression.NotEqual(memberExpression, Expression.Constant(null)),
                        Expression.Call(memberExpression, typeof(string).GetMethod("Contains"), constantExpression));
                case "!contains":
                    return Expression.Or(Expression.Equal(memberExpression, Expression.Constant(null)),
                        Expression.Not(Expression.Call(memberExpression, typeof(string).GetMethod("Contains"), constantExpression)));
                case ">":
                    return Expression.GreaterThan(memberExpression, constantExpression);
                case ">=":
                    return Expression.GreaterThanOrEqual(memberExpression, constantExpression);
                case "<":
                    return Expression.LessThan(memberExpression, constantExpression);
                case "<=":
                    return Expression.LessThanOrEqual(memberExpression, constantExpression);
                default:
                    break;
            }

            throw new Exception($"未匹配的操作符:{compare}");
        }


        /// <summary>
        /// 创建匹配表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="compare"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Expression GetMatchExpression(string name, string compare, string[] values)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (compare == null)
            {
                throw new ArgumentNullException(nameof(compare));
            }

            if (values == null || values.Length == 0)
            {
                throw new ArgumentNullException(nameof(values));
            }

            /*属性表达式*/
            var memberExpression = GetMemberExpression(name, out var propertyInfo);

            /*反射调用静态泛型方法,完成类型转换*/
            //var constantExpression = (ConstantExpression)typeof(ExpressionExpand)
            //        .GetMethod("ToArrayExpression", BindingFlags.Instance |
            //                                      BindingFlags.Static |
            //                                     BindingFlags.Public |
            //                                     BindingFlags.NonPublic |
            //                                     BindingFlags.DeclaredOnly)
            //        .MakeGenericMethod(GetNullableType(propertyInfo.PropertyType))
            //        .Invoke(null, new object[] { values, propertyInfo });

            var constantExpression = ToArrayExpression(values, propertyInfo);


            var methodInfo = typeof(Enumerable).GetMethods()
               .Where(x => x.Name == "Contains")
               .ToArray()[0]
               .MakeGenericMethod(new[] {/* typeof(object) */ propertyInfo.PropertyType });

            var methodCallExpression = Expression.Call(methodInfo, constantExpression, memberExpression);


            switch (compare.ToLower())
            {
                case "in":
                    return methodCallExpression;
                case "!in":
                    return Expression.Not(methodCallExpression);
                default:
                    break;
            }
            throw new Exception($"未匹配的操作符:{compare}");
        }



    }
}
