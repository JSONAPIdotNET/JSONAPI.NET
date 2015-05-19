using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using JSONAPI.Core;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// This transform sorts an IQueryable payload according to query parameters.
    /// </summary>
    public class DefaultSortingTransformer : IQueryableSortingTransformer
    {
        private readonly IModelManager _modelManager;

        /// <summary>
        /// Creates a new SortingQueryableTransformer
        /// </summary>
        /// <param name="modelManager">The model manager used to look up registered type information.</param>
        public DefaultSortingTransformer(IModelManager modelManager)
        {
            _modelManager = modelManager;
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

            foreach (var sortExpression in sortExpressions)
            {
                if (string.IsNullOrWhiteSpace(sortExpression))
                    throw new QueryableTransformException(string.Format("The sort expression \"{0}\" is invalid.", sortExpression));

                var ascending = sortExpression[0] == '+';
                var descending = sortExpression[0] == '-';
                if (!ascending && !descending)
                    throw new QueryableTransformException(string.Format("The sort expression \"{0}\" does not begin with a direction indicator (+ or -).", sortExpression));

                var propertyName = sortExpression.Substring(1);
                if (string.IsNullOrWhiteSpace(propertyName))
                    throw new QueryableTransformException("The property name is missing.");

                var modelProperty = _modelManager.GetPropertyForJsonKey(typeof(T), propertyName);

                if (modelProperty == null)
                    throw new QueryableTransformException(string.Format("The attribute \"{0}\" does not exist on type \"{1}\".",
                        propertyName, _modelManager.GetResourceTypeNameForType(typeof (T))));

                var property = modelProperty.Property;
                
                if (usedProperties.ContainsKey(property))
                    throw new QueryableTransformException(string.Format("The attribute \"{0}\" was specified more than once.", propertyName));

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
