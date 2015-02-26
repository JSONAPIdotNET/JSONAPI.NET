using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Sorts the IQueryable response content according to json-api
    /// </summary>
    public class EnableSortingAttribute : ActionFilterAttribute
    {
        private const string SortQueryParamKey = "sort";

        private readonly IModelManager _modelManager;

        /// <param name="modelManager">The model manager to use to look up model properties by json key name</param>
        public EnableSortingAttribute(IModelManager modelManager)
        {
            _modelManager = modelManager;
        }

        private readonly Lazy<MethodInfo> _openMakeOrderedQueryMethod =
            new Lazy<MethodInfo>(() => typeof(EnableSortingAttribute).GetMethod("MakeOrderedQuery", BindingFlags.NonPublic | BindingFlags.Instance));

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null)
            {
                var objectContent = actionExecutedContext.Response.Content as ObjectContent;
                if (objectContent == null) return;

                var objectType = objectContent.ObjectType;
                if (!objectType.IsGenericType || objectType.GetGenericTypeDefinition() != typeof (IQueryable<>)) return;

                var queryParams = actionExecutedContext.Request.GetQueryNameValuePairs();
                var sortParam = queryParams.FirstOrDefault(kvp => kvp.Key == SortQueryParamKey);
                if (sortParam.Key != SortQueryParamKey) return;

                var queryableElementType = objectType.GenericTypeArguments[0];
                var makeOrderedQueryMethod = _openMakeOrderedQueryMethod.Value.MakeGenericMethod(queryableElementType);

                try
                {
                    var orderedQuery = makeOrderedQueryMethod.Invoke(this, new[] {objectContent.Value, sortParam.Value});

                    actionExecutedContext.Response.Content = new ObjectContent(objectType, orderedQuery,
                        objectContent.Formatter);
                }
                catch (TargetInvocationException ex)
                {
                    var statusCode = ex.InnerException is SortingException
                        ? HttpStatusCode.BadRequest
                        : HttpStatusCode.InternalServerError;
                    throw new HttpResponseException(
                        actionExecutedContext.Request.CreateErrorResponse(statusCode, ex.InnerException.Message));
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private IQueryable<T> MakeOrderedQuery<T>(IQueryable<T> sourceQuery, string sortParam)
        {
            var selectors = new List<Tuple<bool, Expression<Func<T, object>>>>();

            var usedProperties = new Dictionary<PropertyInfo, object>();

            var sortExpressions = sortParam.Split(',');
            foreach (var sortExpression in sortExpressions)
            {
                if (string.IsNullOrWhiteSpace(sortExpression))
                    throw new SortingException(string.Format("The sort expression \"{0}\" is invalid.", sortExpression));

                var ascending = sortExpression[0] == '+';
                var descending = sortExpression[0] == '-';
                if (!ascending && !descending)
                    throw new SortingException(string.Format("The sort expression \"{0}\" does not begin with a direction indicator (+ or -).", sortExpression));

                var propertyName = sortExpression.Substring(1);
                if (string.IsNullOrWhiteSpace(propertyName))
                    throw new SortingException("The property name is missing.");

                var property = _modelManager.GetPropertyForJsonKey(typeof(T), propertyName);

                if (property == null)
                    throw new SortingException(string.Format("The attribute \"{0}\" does not exist on type \"{1}\".",
                        propertyName, _modelManager.GetResourceTypeNameForType(typeof (T))));
                
                if (usedProperties.ContainsKey(property))
                    throw new SortingException(string.Format("The attribute \"{0}\" was specified more than once.", propertyName));

                usedProperties[property] = null;

                var paramExpr = Expression.Parameter(typeof (T));
                var propertyExpr = Expression.Property(paramExpr, property);
                var selector = Expression.Lambda<Func<T, object>>(propertyExpr, paramExpr);

                selectors.Add(Tuple.Create(ascending, selector));
            }

            var firstSelector = selectors.First();

            IOrderedQueryable<T> workingQuery =
                firstSelector.Item1
                    ? sourceQuery.OrderBy(firstSelector.Item2)
                    : sourceQuery.OrderByDescending(firstSelector.Item2);

            return selectors.Skip(1).Aggregate(workingQuery,
                (current, selector) =>
                    selector.Item1 ? current.ThenBy(selector.Item2) : current.ThenByDescending(selector.Item2));
        }
    }

    internal class SortingException : Exception
    {
        public SortingException(string message)
            : base(message)
        {
            
        }
    }
}
