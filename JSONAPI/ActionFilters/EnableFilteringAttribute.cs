using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Filters;

namespace JSONAPI.ActionFilters
{
    public class EnableFilteringAttribute : ActionFilterAttribute
    {
        // Borrowed from http://stackoverflow.com/questions/3631547/select-right-generic-method-with-reflection
        private readonly Lazy<MethodInfo> _whereMethod = new Lazy<MethodInfo>(() =>
            typeof(Queryable).GetMethods()
                .Where(x => x.Name == "Where")
                .Select(x => new { M = x, P = x.GetParameters() })
                .Where(x => x.P.Length == 2
                            && x.P[0].ParameterType.IsGenericType
                            && x.P[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>)
                            && x.P[1].ParameterType.IsGenericType
                            && x.P[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
                .Select(x => new { x.M, A = x.P[1].ParameterType.GetGenericArguments() })
                .Where(x => x.A[0].IsGenericType
                            && x.A[0].GetGenericTypeDefinition() == typeof(Func<,>))
                .Select(x => new { x.M, A = x.A[0].GetGenericArguments() })
                .Where(x => x.A[0].IsGenericParameter
                            && x.A[1] == typeof(bool))
                .Select(x => x.M)
                .SingleOrDefault()
        );

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null)
            {
                var objectContent = actionExecutedContext.Response.Content as ObjectContent;
                if (objectContent != null)
                {
                    var objectType = objectContent.ObjectType;
                    if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                    {
                        var queryableElementType = objectType.GenericTypeArguments[0];
                        var parameter = Expression.Parameter(queryableElementType);
                        var bodyExpr = GetPredicateBody(actionExecutedContext, parameter);
                        var lambdaExpr = Expression.Lambda(bodyExpr, parameter);

                        var genericMethod = _whereMethod.Value.MakeGenericMethod(queryableElementType);
                        var filteredQuery = genericMethod.Invoke(null, new[] { objectContent.Value, lambdaExpr });

                        actionExecutedContext.Response.Content = new ObjectContent(objectType, filteredQuery, objectContent.Formatter);
                    }
                }
            }
        }

        private static Expression GetPredicateBody(HttpActionExecutedContext actionExecutedContext, ParameterExpression param)
        {
            Expression workingExpr = null;

            var type = param.Type;
            var queryPairs = actionExecutedContext.Request.GetQueryNameValuePairs();
            foreach (var queryPair in queryPairs)
            {
                if (String.IsNullOrWhiteSpace(queryPair.Key))
                    continue;

                var prop = type.GetProperty(queryPair.Key) ??
                           type.GetProperty(queryPair.Key.Substring(0, 1).ToUpper() + queryPair.Key.Substring(1));

                if (prop != null)
                {
                    var propertyType = prop.PropertyType;

                    var queryValue = queryPair.Value;
                    if (string.IsNullOrWhiteSpace(queryValue))
                        queryValue = null;

                    Expression expr;
                    if (propertyType == typeof (String))
                    {
                        if (String.IsNullOrWhiteSpace(queryValue))
                        {
                            Expression propertyExpr = Expression.Property(param, prop);
                            expr = Expression.Equal(propertyExpr, Expression.Constant(null));
                        }
                        else
                        {
                            Expression propertyExpr = Expression.Property(param, prop);
                            expr = Expression.Equal(propertyExpr, Expression.Constant(queryValue));
                        }
                    }
                    else if (propertyType == typeof(Boolean))
                    {
                        bool value;
                        expr = bool.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof (Boolean?))
                    {
                        bool tmp;
                        var value = bool.TryParse(queryValue, out tmp) ? tmp : (bool?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(SByte))
                    {
                        SByte value;
                        expr = SByte.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof (SByte?))
                    {
                        SByte tmp;
                        var value = SByte.TryParse(queryValue, out tmp) ? tmp : (SByte?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(Byte))
                    {
                        Byte value;
                        expr = Byte.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(Byte?))
                    {
                        Byte tmp;
                        var value = Byte.TryParse(queryValue, out tmp) ? tmp : (Byte?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(Int16))
                    {
                        Int16 value;
                        expr = Int16.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(Int16?))
                    {
                        Int16 tmp;
                        var value = Int16.TryParse(queryValue, out tmp) ? tmp : (Int16?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(UInt16))
                    {
                        UInt16 value;
                        expr = UInt16.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(UInt16?))
                    {
                        UInt16 tmp;
                        var value = UInt16.TryParse(queryValue, out tmp) ? tmp : (UInt16?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(Int32))
                    {
                        Int32 value;
                        expr = Int32.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(Int32?))
                    {
                        Int32 tmp;
                        var value = Int32.TryParse(queryValue, out tmp) ? tmp : (Int32?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(UInt32))
                    {
                        UInt32 value;
                        expr = UInt32.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(UInt32?))
                    {
                        UInt32 tmp;
                        var value = UInt32.TryParse(queryValue, out tmp) ? tmp : (UInt32?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(Int64))
                    {
                        Int64 value;
                        expr = Int64.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(Int64?))
                    {
                        Int64 tmp;
                        var value = Int64.TryParse(queryValue, out tmp) ? tmp : (Int64?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(UInt64))
                    {
                        UInt64 value;
                        expr = UInt64.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(UInt64?))
                    {
                        UInt64 tmp;
                        var value = UInt64.TryParse(queryValue, out tmp) ? tmp : (UInt64?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(Single))
                    {
                        Single value;
                        expr = Single.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(Single?))
                    {
                        Single tmp;
                        var value = Single.TryParse(queryValue, out tmp) ? tmp : (Single?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(Double))
                    {
                        Double value;
                        expr = Double.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(Double?))
                    {
                        Double tmp;
                        var value = Double.TryParse(queryValue, out tmp) ? tmp : (Double?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(Decimal))
                    {
                        Decimal value;
                        expr = Decimal.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(Decimal?))
                    {
                        Decimal tmp;
                        var value = Decimal.TryParse(queryValue, out tmp) ? tmp : (Decimal?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        DateTime value;
                        expr = DateTime.TryParse(queryValue, out value)
                            ? GetPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof(DateTime?))
                    {
                        DateTime tmp;
                        var value = DateTime.TryParse(queryValue, out tmp) ? tmp : (DateTime?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType == typeof(DateTimeOffset))
                    {
                        DateTimeOffset value;
                        expr = DateTimeOffset.TryParse(queryValue, out value)
                            ? GetPropertyExpression<DateTimeOffset>(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType == typeof (DateTimeOffset?))
                    {
                        DateTimeOffset tmp;
                        var value = DateTimeOffset.TryParse(queryValue, out tmp) ? tmp : (DateTimeOffset?)null;
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else if (propertyType.IsEnum)
                    {
                        int value;
                        expr = (int.TryParse(queryValue, out value) && Enum.IsDefined(propertyType, value))
                            ? GetEnumPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                    }
                    else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>) &&
                             propertyType.GenericTypeArguments[0].IsEnum)
                    {
                        int tmp;
                        var value = int.TryParse(queryValue, out tmp) ? tmp : (int?)null;
                        expr = GetEnumPropertyExpression(value, prop, param);
                    }
                    else
                    {
                        expr = Expression.Constant(true);
                    }

                    workingExpr = workingExpr == null ? expr : Expression.AndAlso(workingExpr, expr);
                }
            }

            return workingExpr ?? Expression.Constant(true); // No filters, so return everything
        }

        private static Expression GetPropertyExpression<T>(T value, PropertyInfo property,
            ParameterExpression param)
        {
            Expression propertyExpr = Expression.Property(param, property);
            var valueExpr = Expression.Constant(value);
            Expression castedConstantExpr = Expression.Convert(valueExpr, typeof(T));
            return Expression.Equal(propertyExpr, castedConstantExpr);
        }

        private static Expression GetEnumPropertyExpression(int? value, PropertyInfo property,
            ParameterExpression param)
        {
            Expression propertyExpr = Expression.Property(param, property);
            var castedValueExpr = Expression.Convert(Expression.Constant(value), typeof(int?));
            var castedPropertyExpr = Expression.Convert(propertyExpr, typeof(int?));
            return Expression.Equal(castedPropertyExpr, castedValueExpr);
        }
    }
}