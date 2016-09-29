using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
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
        private static readonly MethodInfo GetPropertyExpressionMethod = typeof(DefaultFilteringTransformer).GetMethod("GetPropertyExpression", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo GetPropertyExpressionBetweenMethod = typeof(DefaultFilteringTransformer).GetMethod("GetPropertyExpressionBetween", BindingFlags.NonPublic | BindingFlags.Static);


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

        private Expression GetPredicateBodyForProperty(PropertyInfo prop, string queryValue, ParameterExpression param)
        {
            var propertyType = prop.PropertyType;

            Expression expr = null;
            if (propertyType == typeof(string))
            {
                if (string.IsNullOrWhiteSpace(queryValue))
                {
                    Expression propertyExpr = Expression.Property(param, prop);
                    expr = Expression.Equal(propertyExpr, Expression.Constant(null));
                }
                else
                {
                    List<string> parts = new List<string>();
                    if (queryValue.Contains("\""))
                    {
                        queryValue= queryValue.Replace("\"\"", "_#quote#_"); // escaped quotes
                        queryValue = Regex.Replace(queryValue, "\"([^\"]*)\"", delegate (Match match)
                        {
                            string v = match.ToString();
                            v = v.Trim('"');
                            v = v.Replace("_#quote#_", "\""); // restore quotes
                            parts.Add(v);
                            return string.Empty;
                        });
                    } 
                    
                    parts.AddRange(queryValue.Split(','));
                    
                    foreach (var qpart in parts)
                    {
                        Expression innerExpression;
                        // inspired by http://stackoverflow.com/questions/5374481/like-operator-in-linq
                        if (qpart.StartsWith("%") || qpart.EndsWith("%"))
                        {
                            var startWith = qpart.StartsWith("%");
                            var endsWith = qpart.EndsWith("%");
                            string innerPart = qpart;

                            if (startWith) // remove %
                                innerPart = innerPart.Remove(0, 1);

                            if (endsWith) // remove %
                                innerPart = innerPart.Remove(innerPart.Length - 1, 1);

                            var constant = Expression.Constant(innerPart.ToLower());
                            Expression propertyExpr = Expression.Property(param, prop);

                            Expression nullCheckExpression = Expression.NotEqual(propertyExpr, Expression.Constant(null));

                            if (endsWith && startWith)
                            {
                                innerExpression = Expression.AndAlso(nullCheckExpression, Expression.Call(Expression.Call(propertyExpr, ToLowerMethod), ContainsMethod, constant));
                            }
                            else if (startWith)
                            {
                                innerExpression = Expression.AndAlso(nullCheckExpression, Expression.Call(Expression.Call(propertyExpr, ToLowerMethod), EndsWithMethod, constant));
                            }
                            else if (endsWith)
                            {
                                innerExpression = Expression.AndAlso(nullCheckExpression, Expression.Call(Expression.Call(propertyExpr, ToLowerMethod), StartsWithMethod, constant));
                            }
                            else
                            {
                                innerExpression = Expression.Equal(propertyExpr, constant);
                            }
                        }
                        else
                        {
                            Expression propertyExpr = Expression.Property(param, prop);
                            innerExpression = Expression.Equal(propertyExpr, Expression.Constant(qpart));
                        }

                        if (expr == null)
                        {
                            expr = innerExpression;
                        }
                        else
                        {
                            expr = Expression.OrElse(expr, innerExpression);
                        }
                    }
                    
                }
            }
            else if (propertyType.IsEnum)
            {
                if (string.IsNullOrWhiteSpace(queryValue)) // missing enum property
                {
                    expr = Expression.Constant(false);
                }
                else
                {
                    // try to split up for multiple values
                    var parts = queryValue.Split(',');

                    foreach (var part in parts)
                    {
                        int value;
                        var partExpr = (int.TryParse(part, out value) && Enum.IsDefined(propertyType, value))
                            ? GetEnumPropertyExpression(value, prop, param)
                            : Expression.Constant(false);
                        if (expr == null)
                        {
                            expr = partExpr;
                        }
                        else
                        {
                            expr = Expression.OrElse(expr, partExpr);
                        }
                    }
                }
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                     propertyType.GenericTypeArguments[0].IsEnum)
            {
                if (string.IsNullOrWhiteSpace(queryValue))
                {
                    Expression propertyExpr = Expression.Property(param, prop);
                    expr = Expression.Equal(propertyExpr, Expression.Constant(null));
                }
                else
                {
                    // try to split up for multiple values
                    var parts = queryValue.Split(',');

                    foreach (var part in parts)
                    {
                        int tmp;
                        var value = int.TryParse(part, out tmp) ? tmp : (int?)null;
                        var partExpr = GetEnumPropertyExpression(value, prop, param);
                        if (expr == null)
                        {
                            expr = partExpr;
                        }
                        else
                        {
                            expr = Expression.OrElse(expr, partExpr);
                        }
                    }
                }
            }
            else if (Nullable.GetUnderlyingType(propertyType) != null) // It's nullable
            {
                expr = GetExpressionNullable(queryValue, prop, propertyType, param);
            }
            else
            {
                expr = GetExpression(queryValue, prop, propertyType, param);
                if (expr == null)
                {
                    expr = Expression.Constant(true);
                }
            }

            return expr;
        }

        private Expression GetExpressionNullable(string queryValue, PropertyInfo prop, Type propertyType, ParameterExpression param)
        {
            Type underlayingType = Nullable.GetUnderlyingType(propertyType);
            try
            {

                var methodInfo = GetPropertyExpressionMethod.MakeGenericMethod(propertyType);

                if (queryValue == null)
                {
                    return (Expression)methodInfo.Invoke(null, new object[] { null, prop, param });
                }

                // try to split up for multiple values
                var parts = queryValue.Split(',');

                if (underlayingType == typeof(DateTime) || underlayingType == typeof(DateTimeOffset))
                {
                    return GetDateReangeExpression(parts, prop, underlayingType, propertyType, param);
                }


                Expression expr = null;
                foreach (var part in parts)
                {
                    TypeConverter conv =TypeDescriptor.GetConverter(underlayingType);
                    var value = conv.ConvertFromInvariantString(part);

                    if (expr == null)
                    {
                        expr = (Expression) methodInfo.Invoke(null, new[] { value, prop, param });
                    }
                    else
                    {
                        expr = Expression.OrElse(expr, (Expression)methodInfo.Invoke(null, new[] { value, prop, param }));
                    }
                }
                return expr;
            }
            catch (NotSupportedException)
            {
                return Expression.Constant(false);
            }

        }

        private Expression GetDateReangeExpression(string[] parts, PropertyInfo prop, Type underlyingType, Type propertyType, ParameterExpression param)
        {
            Expression expr = null;
            foreach (var part in parts)
            {
                var mode = "";
                if (!part.Contains("-"))
                    mode = "year";
                if (part.Contains("-"))
                    mode = "month";
                if (part.Count(x => x.Equals('-')) == 2)
                {
                    mode = "day";
                    if (part.Contains(" ")) // there is a time
                    {
                        mode = "hour";
                        if (part.Contains(":"))
                            mode = "minute";
                        if (part.Count(x => x.Equals(':')) == 2)
                        {
                            mode = "second";
                        }
                    }
                }
                var partToParse = part;

                // make the datetime valid
                if (mode == "year")
                    partToParse += "-01-01";
                if (mode == "hour")
                    partToParse += ":00";

                TypeConverter conv = TypeDescriptor.GetConverter(underlyingType);
                dynamic value = conv.ConvertFromInvariantString(partToParse);
                var upper =value;
                switch (mode)
                {
                    case "year":
                        upper= upper.AddYears(1);
                        break;
                    case "month":
                        upper = upper.AddMonths(1);
                        break;
                    case "day":
                        upper = upper.AddDays(1);
                        break;
                    case "hour":
                        upper = upper.AddHours(1);
                        break;
                    case "minute":
                        upper = upper.AddMinutes(1);
                        break;
                    case "second":
                        upper = upper.AddSeconds(1);
                        break;
                }
                upper = upper.AddTicks(-1);
                var methodInfo = GetPropertyExpressionBetweenMethod.MakeGenericMethod(propertyType);
                Expression innerExpr = (Expression)methodInfo.Invoke(null, new object[] {value, upper, prop, param});
                if (expr == null)
                {
                    expr = innerExpr;
                }
                else
                {
                    expr = Expression.OrElse(expr, innerExpr);
                }

            }
            return expr;
        }


        private Expression GetExpression(string queryValue, PropertyInfo prop, Type propertyType, ParameterExpression param)
        {
            try
            {
                if(queryValue == null) // missing property
                    return Expression.Constant(false);

                var parts = queryValue.Split(',');
                Expression expr = null;
                foreach (var part in parts)
                {
                    dynamic value = TypeDescriptor.GetConverter(propertyType).ConvertFromInvariantString(part);
                    if (expr == null)
                    {
                        expr = GetPropertyExpression(value, prop, param);
                    }
                    else
                    {
                        expr = Expression.OrElse(expr, GetPropertyExpression(value, prop, param));
                    }
                }
                return expr;
            }
            catch (NotSupportedException)
            {
                return Expression.Constant(false);
            }
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

        private static Expression GetPropertyExpressionBetween<T>(T lowerValue, T upperValue, PropertyInfo property,
            ParameterExpression param)
        {
            Expression propertyExpr = Expression.Property(param, property);
            var lowerValueExpr = Expression.Constant(lowerValue);
            var upperValueExpr = Expression.Constant(upperValue);
            Expression lowerCastedConstantExpr = Expression.Convert(lowerValueExpr, typeof(T));
            Expression upperCastedConstantExpr = Expression.Convert(upperValueExpr, typeof(T));
            return Expression.AndAlso(Expression.GreaterThanOrEqual(propertyExpr, lowerCastedConstantExpr), Expression.LessThanOrEqual(propertyExpr, upperCastedConstantExpr));
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
