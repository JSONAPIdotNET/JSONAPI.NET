using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using JSONAPI.Core;
using JSONAPI.Documents.Builders;

namespace JSONAPI.QueryableTransformers
{
    /// <summary>
    /// This transformer filters an IQueryable based on query-string values.
    /// </summary>
    public class DefaultFilteringTransformer : IQueryableFilteringTransformer
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;

        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains");
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower", new Type[] {});



        /// <summary>
        /// Creates a new FilteringQueryableTransformer
        /// </summary>
        /// <param name="resourceTypeRegistry">The registry used to look up registered type information.</param>
        public DefaultFilteringTransformer(IResourceTypeRegistry resourceTypeRegistry)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
        }

        public IQueryable<T> Filter<T>(IQueryable<T> query, HttpRequestMessage request)
        {
            var parameter = Expression.Parameter(typeof(T));
            var bodyExpr = GetPredicateBody(request, parameter);
            var lambdaExpr = Expression.Lambda<Func<T, bool>>(bodyExpr, parameter);
            return query.Where(lambdaExpr);
        }

        // Borrowed from http://stackoverflow.com/questions/3631547/select-right-generic-method-with-reflection
        // ReSharper disable once UnusedMember.Local
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

        private Expression GetPredicateBody(HttpRequestMessage request, ParameterExpression param)
        {
            Expression workingExpr = null;

            var type = param.Type;
            var queryPairs = request.GetQueryNameValuePairs();
            foreach (var queryPair in queryPairs)
            {
                if (String.IsNullOrWhiteSpace(queryPair.Key))
                    continue;

                if (!queryPair.Key.StartsWith("filter."))
                    continue;

                var filterField = queryPair.Key.Substring(7); // Skip "filter."

                IResourceTypeRegistration registration;
                try
                {
                    registration = _resourceTypeRegistry.GetRegistrationForType(type);
                }
                catch (TypeRegistrationNotFoundException)
                {
                    throw JsonApiException.CreateForBadRequest("No registration exists for the specified type");
                }

                var expr = GetPredicate(filterField, registration, param, queryPair.Value);
                workingExpr = workingExpr == null ? expr : Expression.AndAlso(workingExpr, expr);
            }

            return workingExpr ?? Expression.Constant(true); // No filters, so return everything
        }

        private Expression GetPredicate(string filterField, IResourceTypeRegistration registration, ParameterExpression param, string queryValue)
        {
            if (filterField == "id")
                return GetPredicateBodyForProperty(registration.IdProperty, queryValue, param);

            var resourceTypeField = registration.GetFieldByName(filterField);
            if (resourceTypeField == null)
                throw JsonApiException.CreateForBadRequest(
                    string.Format("No attribute {0} exists on the specified type.", filterField));

            if (string.IsNullOrWhiteSpace(queryValue))
                queryValue = null;

            // See if it is a field property
            var fieldModelProperty = resourceTypeField as ResourceTypeAttribute;
            if (fieldModelProperty != null)
                return GetPredicateBodyForField(fieldModelProperty, queryValue, param);

            // See if it is a relationship property
            var relationshipModelProperty = resourceTypeField as ResourceTypeRelationship;
            if (relationshipModelProperty != null)
                return GetPredicateBodyForRelationship(relationshipModelProperty, queryValue, param);

            throw JsonApiException.CreateForBadRequest(
                string.Format("The attribute {0} is unsupported for filtering.", filterField));
        }
        
        private Expression GetPredicateBodyForField(ResourceTypeAttribute resourceTypeAttribute, string queryValue,
            ParameterExpression param)
        {
            return GetPredicateBodyForProperty(resourceTypeAttribute.Property, queryValue, param);
        }

        // ReSharper disable once FunctionComplexityOverflow
        // TODO: should probably break this method up
        private Expression GetPredicateBodyForProperty(PropertyInfo prop, string queryValue, ParameterExpression param)
        {
            var propertyType = prop.PropertyType;

            Expression expr;
            if (propertyType == typeof(String))
            {
                if (String.IsNullOrWhiteSpace(queryValue))
                {
                    Expression propertyExpr = Expression.Property(param, prop);
                    expr = Expression.Equal(propertyExpr, Expression.Constant(null));
                }
                else
                { // inspired by http://stackoverflow.com/questions/5374481/like-operator-in-linq
                    if (queryValue.StartsWith("%") || queryValue.EndsWith("%"))
                    {
                        var startWith = queryValue.StartsWith("%");
                        var endsWith = queryValue.EndsWith("%");

                        if (startWith) // remove %
                            queryValue = queryValue.Remove(0, 1);

                        if (endsWith) // remove %
                            queryValue = queryValue.Remove(queryValue.Length - 1, 1);

                        var constant = Expression.Constant(queryValue.ToLower());
                        Expression propertyExpr = Expression.Property(param, prop);

                        Expression nullCheckExpression = Expression.NotEqual(propertyExpr, Expression.Constant(null));

                        if (endsWith && startWith)
                        {
                            expr = Expression.AndAlso(nullCheckExpression, Expression.Call(Expression.Call(propertyExpr,ToLowerMethod), ContainsMethod, constant));
                        }
                        else if (startWith)
                        {
                            expr = Expression.AndAlso(nullCheckExpression, Expression.Call(Expression.Call(propertyExpr, ToLowerMethod), EndsWithMethod, constant));
                        }
                        else if (endsWith)
                        {
                            expr = Expression.AndAlso(nullCheckExpression, Expression.Call(Expression.Call(propertyExpr, ToLowerMethod), StartsWithMethod, constant));
                        }
                        else
                        {
                            expr = Expression.Equal(propertyExpr, constant);
                        }
                    }
                    else
                    {
                        Expression propertyExpr = Expression.Property(param, prop);
                        expr = Expression.Equal(propertyExpr, Expression.Constant(queryValue));
                    }
                }
            }
            else if (propertyType == typeof(Boolean))
            {
                bool value;
                expr = bool.TryParse(queryValue, out value)
                    ? GetPropertyExpression(value, prop, param)
                    : Expression.Constant(false);
            }
            else if (propertyType == typeof(Boolean?))
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
            else if (propertyType == typeof(SByte?))
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
                expr = Single.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
                    ? GetPropertyExpression(value, prop, param)
                    : Expression.Constant(false);
            }
            else if (propertyType == typeof(Single?))
            {
                Single tmp;
                var value = Single.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp) ? tmp : (Single?)null;
                expr = GetPropertyExpression(value, prop, param);
            }
            else if (propertyType == typeof(Double))
            {
                Double value;
                expr = Double.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
                    ? GetPropertyExpression(value, prop, param)
                    : Expression.Constant(false);
            }
            else if (propertyType == typeof(Double?))
            {
                Double tmp;
                var value = Double.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp) ? tmp : (Double?)null;
                expr = GetPropertyExpression(value, prop, param);
            }
            else if (propertyType == typeof(Decimal))
            {
                Decimal value;
                expr = Decimal.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
                    ? GetPropertyExpression(value, prop, param)
                    : Expression.Constant(false);
            }
            else if (propertyType == typeof(Decimal?))
            {
                Decimal tmp;
                var value = Decimal.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp) ? tmp : (Decimal?)null;
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
            else if (propertyType == typeof(DateTimeOffset?))
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
                var value = int.TryParse(queryValue, out tmp) ? tmp : (int?) null;
                expr = GetEnumPropertyExpression(value, prop, param);
            }
            else
            {
                expr = Expression.Constant(true);
            }

            return expr;
        }

        private Expression GetPredicateBodyForRelationship(ResourceTypeRelationship resourceTypeProperty, string queryValue, ParameterExpression param)
        {
            var relatedType = resourceTypeProperty.RelatedType;
            PropertyInfo relatedIdProperty;
            try
            {
                var registration = _resourceTypeRegistry.GetRegistrationForType(relatedType);
                relatedIdProperty = registration.IdProperty;
            }
            catch (TypeRegistrationNotFoundException)
            {
                throw JsonApiException.CreateForBadRequest("No registration exists for the specified type");
            }

            var prop = resourceTypeProperty.Property;

            if (resourceTypeProperty.IsToMany)
            {
                var propertyExpr = Expression.Property(param, prop);

                if (string.IsNullOrWhiteSpace(queryValue))
                {
                    var leftExpr = Expression.Equal(propertyExpr, Expression.Constant(null));

                    var asQueryableCallExpr = Expression.Call(
                        typeof(Queryable),
                        "AsQueryable",
                        new[] { relatedType },
                        propertyExpr);
                    var anyCallExpr = Expression.Call(
                        typeof(Queryable),
                        "Any",
                        new[] { relatedType },
                        asQueryableCallExpr);
                    var rightExpr = Expression.Not(anyCallExpr);

                    return Expression.OrElse(leftExpr, rightExpr);
                }
                else
                {
                    var leftExpr = Expression.NotEqual(propertyExpr, Expression.Constant(null));

                    var idValue = queryValue.Trim();
                    var idExpr = Expression.Constant(idValue);
                    var anyParam = Expression.Parameter(relatedType);
                    var relatedIdPropertyExpr = Expression.Property(anyParam, relatedIdProperty);
                    var relatedIdPropertyEqualsIdExpr = Expression.Equal(relatedIdPropertyExpr, idExpr);
                    var anyPredicateExpr = Expression.Lambda(relatedIdPropertyEqualsIdExpr, anyParam);
                    var asQueryableCallExpr = Expression.Call(
                        typeof(Queryable),
                        "AsQueryable",
                        new[] { relatedType },
                        propertyExpr);
                    var rightExpr = Expression.Call(
                        typeof(Queryable),
                        "Any",
                        new[] { relatedType },
                        asQueryableCallExpr,
                        anyPredicateExpr);

                    return Expression.AndAlso(leftExpr, rightExpr);
                }
            }
            else
            {
                var propertyExpr = Expression.Property(param, prop);

                if (string.IsNullOrWhiteSpace(queryValue))
                    return Expression.Equal(propertyExpr, Expression.Constant(null));

                var leftExpr = Expression.NotEqual(propertyExpr, Expression.Constant(null));

                var idValue = queryValue.Trim();
                var idExpr = Expression.Constant(idValue);
                var relatedIdPropertyExpr = Expression.Property(propertyExpr, relatedIdProperty);
                var rightExpr = Expression.Equal(relatedIdPropertyExpr, idExpr);

                return Expression.AndAlso(leftExpr, rightExpr);
            }
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
