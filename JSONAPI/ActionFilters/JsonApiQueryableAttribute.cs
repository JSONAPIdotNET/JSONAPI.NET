using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// This is an action filter that you can insert into your Web API pipeline to perform various transforms
    /// on IQueryable payloads.
    /// </summary>
    public class JsonApiQueryableAttribute : ActionFilterAttribute
    {
        private readonly IQueryableEnumerationTransformer _enumerationTransformer;
        private readonly IQueryableFilteringTransformer _filteringTransformer;
        private readonly IQueryableSortingTransformer _sortingTransformer;
        private readonly IQueryablePaginationTransformer _paginationTransformer;

        /// <summary>
        /// Creates a new JsonApiQueryableAttribute.
        /// </summary>
        /// <param name="enumerationTransformer">The transform to be used for enumerating IQueryable payloads.</param>
        /// <param name="filteringTransformer">The transform to be used for filtering IQueryable payloads</param>
        /// <param name="sortingTransformer">The transform to be used for sorting IQueryable payloads.</param>
        /// <param name="paginationTransformer">The transform to be used for pagination of IQueryable payloads.</param>
        public JsonApiQueryableAttribute(
            IQueryableEnumerationTransformer enumerationTransformer,
            IQueryableFilteringTransformer filteringTransformer = null,
            IQueryableSortingTransformer sortingTransformer = null,
            IQueryablePaginationTransformer paginationTransformer = null)
        {
            _sortingTransformer = sortingTransformer;
            _paginationTransformer = paginationTransformer;
            _enumerationTransformer = enumerationTransformer;
            _filteringTransformer = filteringTransformer;
        }

        private readonly Lazy<MethodInfo> _openApplyTransformsMethod =
            new Lazy<MethodInfo>(() => typeof(JsonApiQueryableAttribute).GetMethod("ApplyTransforms", BindingFlags.NonPublic | BindingFlags.Instance));

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
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
                        var applyTransformsMethod = _openApplyTransformsMethod.Value.MakeGenericMethod(queryableElementType);

                        try
                        {
                            dynamic materializedQueryTask;
                            try
                            {
                                materializedQueryTask = applyTransformsMethod.Invoke(this,
                                    new[] {objectContent.Value, actionExecutedContext.Request, cancellationToken});
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw ex.InnerException;
                            }

                            var materializedResults = await materializedQueryTask;

                            actionExecutedContext.Response.Content = new ObjectContent(
                                materializedResults.GetType(),
                                materializedResults,
                                objectContent.Formatter);
                        }
                        catch (HttpResponseException)
                        {
                            throw;
                        }
                        catch (QueryableTransformException ex)
                        {
                            throw new HttpResponseException(
                                actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
                        }
                        catch (Exception ex)
                        {
                            throw new HttpResponseException(
                                actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
                            
                        }
                    }
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private async Task<T[]> ApplyTransforms<T>(IQueryable<T> query, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_filteringTransformer != null)
                query = _filteringTransformer.Filter(query, request);

            if (_sortingTransformer != null)
                query = _sortingTransformer.Sort(query, request);

            if (_paginationTransformer != null)
                query = _paginationTransformer.ApplyPagination(query, request);

            return await _enumerationTransformer.Enumerate(query, cancellationToken);
        }
    }
}
