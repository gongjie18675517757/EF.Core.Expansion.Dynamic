using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EF.Core.Expansion.Dynamic
{
    public class ExpressionExpand
    {
        private static Dictionary<Type, MethodInfo> keyValuePairs = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> typePropertyDic = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

         

        /// <summary>
        /// 获取属性集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, PropertyInfo> GetPropertyDic<T>()
        {
            var type = typeof(T);
            return GetPropertyDic(type);
        }

        /// <summary>
        /// 获取属性集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Dictionary<string, PropertyInfo> GetPropertyDic(Type type)
        {
            lock (typePropertyDic)
            {
                if (!typePropertyDic.TryGetValue(type, out var propertyDic))
                {
                    propertyDic = type.GetProperties().ToDictionary(x => x.Name.ToLower(), x => x);
                    typePropertyDic.Add(type, propertyDic);
                }
                return propertyDic;
            }
        }

        /// <summary>
        /// 组装多个表达式
        /// </summary>
        /// <param name="multipleMark"></param>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static Expression MergeExpression(MultipleMark multipleMark, IEnumerable<Expression> expressions)
        {
            if (expressions == null || !expressions.Any())
            {
                throw new ArgumentNullException(nameof(expressions));
            }

            Func<Expression, Expression, BinaryExpression> func = null;
            switch (multipleMark)
            {
                case MultipleMark.And:
                    func = Expression.And;
                    break;
                case MultipleMark.Or:
                    func = Expression.Or;
                    break;
            }

            Expression expression = null;
            foreach (var item in expressions)
            {
                if (expression == null)
                    expression = item;
                else
                    expression = func(expression, item);
            }

            return expression;

        }

        /// <summary>
        /// 值[集合]表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private static ConstantExpression ToArrayExpression<T>(string[] values, PropertyInfo propertyInfo)
        {
            var resultValues = new List<T>();

            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (!ConvertType(propertyInfo.PropertyType, value, out var result))
                    throw new Exception($"属性:{propertyInfo.Name}的值:{value}无法转换成{propertyInfo.PropertyType.Name}类型");

                resultValues.Add((T)result);
            }

            var constantExpression = Expression.Constant(resultValues.ToArray());
            return constantExpression;
        }

       
        public static ConstantExpression ToArrayExpression(string[] values, PropertyInfo propertyInfo)
        {
            lock (keyValuePairs)
            {
                var type = GetNullableType(propertyInfo.PropertyType);

                if (!keyValuePairs.TryGetValue(type,out var methodInfo))
                {
                    methodInfo = typeof(ExpressionExpand)
                         .GetMethod("ToArrayExpression", BindingFlags.Instance |
                                                  BindingFlags.Static |
                                                 //BindingFlags.Public |
                                                 BindingFlags.NonPublic |
                                                 BindingFlags.DeclaredOnly)
                         .MakeGenericMethod(GetNullableType(propertyInfo.PropertyType));

                    keyValuePairs.Add(propertyInfo.PropertyType, methodInfo); 
                }

                return (ConstantExpression)methodInfo.Invoke(null, new object[] { values, propertyInfo });
            }
        
        }

        /// <summary>
        /// 获取类型的非空类型
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public static Type GetNullableType(Type tp)
        {
            if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Nullable<>))
                return tp.GetGenericArguments()[0];

            return tp;
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="val"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ConvertType(Type tp, object val, out object result)
        {
            result = val;
            if (val == null) return false;

            //泛型Nullable判断，取其中的类型
            tp = GetNullableType(tp);

            //string直接返回转换
            if (tp.Name.ToLower() == "string")
            {
                return true;
            }
            //反射获取TryParse方法
            var TryParse = tp.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
                                            new Type[] { typeof(string), tp.MakeByRefType() },
                                            new ParameterModifier[] { new ParameterModifier(2) });
            var parameters = new object[] { val, Activator.CreateInstance(tp) };
            bool success = (bool)TryParse.Invoke(null, parameters);
            //成功返回转换后的值，否则返回类型的默认值
            if (success)
            {
                result = parameters[1];
                return true;
            }
            return false;
        }
    }
}
