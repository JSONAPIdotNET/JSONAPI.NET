using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using JSONAPI.Core;
using JSONAPI.Payload.Builders;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// This transform sorts an IQueryable payload according to query parameters.
    /// </summary>
    public class DefaultSortingTransformer : IQueryableSortingTransformer
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;

        /// <summary>
        /// Creates a new SortingQueryableTransformer
        /// </summary>
        /// <param name="resourceTypeRegistry">The model manager used to look up registered type information.</param>
        public DefaultSortingTransformer(IResourceTypeRegistry resourceTypeRegistry)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
        }
        
        private const string SortQueryParamKey = "sort";

        public IOrderedQueryable<T> Sort<T>(IQueryable<T> query, HttpRequestMessage request)
        {
            var queryParams = request.GetQueryNameValuePairs();
            var sortParam = queryParams.FirstOrDefault(kvp => kvp.Key == SortQueryParamKey);

            string[] sortExpressions;
            if (sortParam.Key != SortQueryParamKey)
            {
                sortExpressions = new[] { "+id" }; // We have to sort by something, so make it the ID.
            }
            else
            {
                sortExpressions = sortParam.Value.Split(',');
            }

            var selectors = new List<Tuple<bool, Expression<Func<T, object>>>>();
            var usedProperties = new Dictionary<PropertyInfo, object>();
            
            var registration = _resourceTypeRegistry.GetRegistrationForType(typeof (T));

            foreach (var sortExpression in sortExpressions)
            {
                if (string.IsNullOrWhiteSpace(sortExpression))
                    throw JsonApiException.CreateForParameterError("Invalid sort expression", string.Format("The sort expression \"{0}\" is invalid.", sortExpression), "sort");

                var ascending = sortExpression[0] == '+';
                var descending = sortExpression[0] == '-';
                if (!ascending && !descending)
                    throw JsonApiException.CreateForParameterError("Cannot determine sort direction",
                        string.Format(
                            "The sort expression \"{0}\" does not begin with a direction indicator (+ or -).",
                            sortExpression), "sort");

                var propertyName = sortExpression.Substring(1);
                if (string.IsNullOrWhiteSpace(propertyName))
                    throw JsonApiException.CreateForParameterError("Empty sort expression", "One of the sort expressions is empty.", "sort");

                PropertyInfo property;
                if (propertyName == "id")
                {
                    property = registration.IdProperty;
                }
                else
                {
                    var modelProperty = registration.GetFieldByName(propertyName);
                    if (modelProperty == null)
                        throw JsonApiException.CreateForParameterError("Attribute not found",
                            string.Format("The attribute \"{0}\" does not exist on type \"{1}\".",
                                propertyName, registration.ResourceTypeName), "sort");

                    property = modelProperty.Property;
                }

                
                if (usedProperties.ContainsKey(property))
                    throw JsonApiException.CreateForParameterError("Attribute specified more than once",
                        string.Format("The attribute \"{0}\" was specified more than once.", propertyName), "sort");

                usedProperties[property] = null;

                var paramExpr = Expression.Parameter(typeof (T));
                var propertyExpr = Expression.Property(paramExpr, property);
                var selector = Expression.Lambda<Func<T, object>>(propertyExpr, paramExpr);

                selectors.Add(Tuple.Create(ascending, selector));
            }

            var firstSelector = selectors.First();

            IOrderedQueryable<T> workingQuery =
                firstSelector.Item1
                    ? query.OrderBy(firstSelector.Item2)
                    : query.OrderByDescending(firstSelector.Item2);

            return selectors.Skip(1).Aggregate(workingQuery,
                (current, selector) =>
                    selector.Item1 ? current.ThenBy(selector.Item2) : current.ThenByDescending(selector.Item2));
        }
    }
}
