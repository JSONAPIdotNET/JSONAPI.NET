using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using JSONAPI.Core;
using JSONAPI.Documents.Builders;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// This transform sorts an IQueryable according to query parameters.
    /// </summary>
    public class DefaultSortingTransformer : IQueryableSortingTransformer
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;

        /// <summary>
        /// Creates a new SortingQueryableTransformer
        /// </summary>
        /// <param name="resourceTypeRegistry">The registry used to look up registered type information.</param>
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
                sortExpressions = new[] { "id" }; // We have to sort by something, so make it the ID.
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
                if (string.IsNullOrEmpty(sortExpression))
                    throw JsonApiException.CreateForParameterError("Empty sort expression", "One of the sort expressions is empty.", "sort");

                bool ascending;
                string fieldName;
                if (sortExpression[0] == '-')
                {
                    ascending = false;
                    fieldName = sortExpression.Substring(1);
                }
                else
                {
                    ascending = true;
                    fieldName = sortExpression;
                }

                if (string.IsNullOrWhiteSpace(fieldName))
                    throw JsonApiException.CreateForParameterError("Empty sort expression", "One of the sort expressions is empty.", "sort");

                var paramExpr = Expression.Parameter(typeof(T));
                Expression sortValueExpression;

                if (fieldName == "id")
                {
                    sortValueExpression = registration.GetSortByIdExpression(paramExpr);
                }
                else
                {
                    var modelProperty = registration.GetFieldByName(fieldName);
                    if (modelProperty == null)
                        throw JsonApiException.CreateForParameterError("Attribute not found",
                            string.Format("The attribute \"{0}\" does not exist on type \"{1}\".",
                                fieldName, registration.ResourceTypeName), "sort");

                    var property = modelProperty.Property;
                    
                    if (usedProperties.ContainsKey(property))
                        throw JsonApiException.CreateForParameterError("Attribute specified more than once",
                            string.Format("The attribute \"{0}\" was specified more than once.", fieldName), "sort");

                    usedProperties[property] = null;

                    sortValueExpression = Expression.Property(paramExpr, property);
                }

                var selector = Expression.Lambda<Func<T, object>>(sortValueExpression, paramExpr);
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
