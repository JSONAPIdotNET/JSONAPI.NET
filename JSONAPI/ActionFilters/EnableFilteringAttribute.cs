using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Filters;
using JSONAPI.Core;

namespace JSONAPI.ActionFilters
{
    public class EnableFilteringAttribute : ActionFilterAttribute
    {
        private readonly IModelManager _modelManager;

        public EnableFilteringAttribute(IModelManager modelManager)
        {
            _modelManager = modelManager;
        }

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
                        var bodyExpr = GetPredicateBody(actionExecutedContext.Request, parameter);
                        var lambdaExpr = Expression.Lambda(bodyExpr, parameter);

                        var genericMethod = _whereMethod.Value.MakeGenericMethod(queryableElementType);
                        var filteredQuery = genericMethod.Invoke(null, new[] { objectContent.Value, lambdaExpr });

                        actionExecutedContext.Response.Content = new ObjectContent(objectType, filteredQuery, objectContent.Formatter);
                    }
                }
            }
        }

        private Expression GetPredicateBody(HttpRequestMessage request, ParameterExpression param)
        {
            Expression workingExpr = null;

            var type = param.Type;
            var queryPairs = request.GetQueryNameValuePairs();
            foreach (var queryPair in queryPairs)
            {
                if (String.IsNullOrWhiteSpace(queryPair.Key))
                    continue;

                ModelProperty modelProperty;
                try
                {
                    modelProperty = _modelManager.GetPropertyForJsonKey(type, queryPair.Key);
                }
                catch (InvalidOperationException)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
                
                var queryValue = queryPair.Value;
                if (string.IsNullOrWhiteSpace(queryValue))
                    queryValue = null;

                Expression expr = null;

                // See if it is a field property
                var fieldModelProperty = modelProperty as FieldModelProperty;
                if (fieldModelProperty != null)
                    expr = GetPredicateBodyForField(fieldModelProperty, queryValue, param);

                // See if it is a relationship property
                var relationshipModelProperty = modelProperty as RelationshipModelProperty;
                if (relationshipModelProperty != null)
                    expr = GetPredicateBodyForRelationship(relationshipModelProperty, queryValue, param);

                workingExpr = workingExpr == null ? expr : Expression.AndAlso(workingExpr, expr);
            }

            return workingExpr ?? Expression.Constant(true); // No filters, so return everything
        }

        private Expression GetPredicateBodyForField(FieldModelProperty modelProperty, string queryValue, ParameterExpression param)
        {
            var prop = modelProperty.Property;
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

        private Expression GetPredicateBodyForRelationship(RelationshipModelProperty modelProperty, string queryValue, ParameterExpression param)
        {
            var relatedType = modelProperty.RelatedType;
            PropertyInfo relatedIdProperty;
            try
            {
                relatedIdProperty = _modelManager.GetIdProperty(relatedType);
            }
            catch (InvalidOperationException)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var prop = modelProperty.Property;

            if (modelProperty.IsToMany)
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