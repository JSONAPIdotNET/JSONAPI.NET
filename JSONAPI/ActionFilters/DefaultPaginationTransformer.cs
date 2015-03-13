using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Performs pagination a
    /// </summary>
    public class DefaultPaginationTransformer : IQueryablePaginationTransformer
    {
        private const int DefaultPageSize = 100;

        private readonly string _pageNumberQueryParam;
        private readonly string _pageSizeQueryParam;
        private readonly int? _maxPageSize;

        /// <summary>
        /// Creates a DefaultPaginationTransformer
        /// </summary>
        /// <param name="pageNumberQueryParam">The query parameter to use to indicate the page number</param>
        /// <param name="pageSizeQueryParam">The query parameter to use to indicate the page size</param>
        /// <param name="maxPageSize">The maximum page size to allow clients to request. Leave null for no restriction.</param>
        public DefaultPaginationTransformer(string pageNumberQueryParam, string pageSizeQueryParam, int? maxPageSize = null)
        {
            if (maxPageSize <= 0) throw new ArgumentOutOfRangeException("maxPageSize", "The maximum page size must be 1 or greater.");

            _pageNumberQueryParam = pageNumberQueryParam;
            _pageSizeQueryParam = pageSizeQueryParam;
            _maxPageSize = maxPageSize;
        }

        public IQueryable<T> ApplyPagination<T>(IQueryable<T> query, HttpRequestMessage request)
        {
            var hasPageNumberParam = false;
            var hasPageSizeParam = false;
            var pageNumber = 0;
            var pageSize = _maxPageSize ?? DefaultPageSize;
            foreach (var kvp in request.GetQueryNameValuePairs())
            {
                if (kvp.Key == _pageNumberQueryParam)
                {
                    hasPageNumberParam = true;
                    if (!int.TryParse(kvp.Value, out pageNumber)) throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
                else if (kvp.Key == _pageSizeQueryParam)
                {
                    hasPageSizeParam = true;
                    if (!int.TryParse(kvp.Value, out pageSize)) throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
            }

            if (!hasPageNumberParam && !hasPageSizeParam)
                return query;

            if ((hasPageNumberParam && !hasPageSizeParam) || (!hasPageNumberParam && hasPageSizeParam))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            if (pageNumber < 0 || pageSize < 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            if (_maxPageSize != null && pageSize > _maxPageSize.Value)
                pageSize = _maxPageSize.Value;

            var skip = pageNumber * pageSize;
            return query.Skip(skip).Take(pageSize);
        }
    }
}
