using System;
using System.Linq;
using System.Net.Http;
using JSONAPI.ActionFilters;
using JSONAPI.Documents.Builders;

namespace JSONAPI.QueryableTransformers
{
    /// <summary>
    /// Performs pagination
    /// </summary>
    public class DefaultPaginationTransformer : IQueryablePaginationTransformer
    {
        private const int DefaultPageSize = 100;
        private const string PageNumberQueryParam = "page.number";
        private const string PageSizeQueryParam = "page.size";

        private readonly int? _maxPageSize;

        /// <summary>
        /// Creates a DefaultPaginationTransformer
        /// </summary>
        /// <param name="maxPageSize">The maximum page size to allow clients to request. Leave null for no restriction.</param>
        public DefaultPaginationTransformer(int? maxPageSize = null)
        {
            if (maxPageSize <= 0) throw new ArgumentOutOfRangeException("maxPageSize", "The maximum page size must be 1 or greater.");

            _maxPageSize = maxPageSize;
        }

        public IPaginationTransformResult<T> ApplyPagination<T>(IQueryable<T> query, HttpRequestMessage request)
        {
            var hasPageNumberParam = false;
            var hasPageSizeParam = false;
            var pageNumber = 0;
            var pageSize = _maxPageSize ?? DefaultPageSize;
            foreach (var kvp in request.GetQueryNameValuePairs())
            {
                if (kvp.Key == PageNumberQueryParam)
                {
                    hasPageNumberParam = true;
                    if (!int.TryParse(kvp.Value, out pageNumber))
                        throw JsonApiException.CreateForParameterError("Invalid page number",
                            "Page number must be a positive integer.", PageNumberQueryParam);

                }
                else if (kvp.Key == PageSizeQueryParam)
                {
                    hasPageSizeParam = true;
                    if (!int.TryParse(kvp.Value, out pageSize))
                        throw JsonApiException.CreateForParameterError("Invalid page size",
                            "Page size must be a positive integer.", PageSizeQueryParam);
                }
            }

            if (!hasPageNumberParam && !hasPageSizeParam)
            {
                return new DefaultPaginationTransformResult<T>
                {
                    PagedQuery = query,
                    PaginationWasApplied = false
                };
            }

            if ((hasPageNumberParam && !hasPageSizeParam))
                throw JsonApiException.CreateForParameterError("Page size missing",
                    string.Format("In order for paging to work properly, if either {0} or {1} is set, both must be.",
                        PageNumberQueryParam, PageSizeQueryParam), PageNumberQueryParam);

            if ((!hasPageNumberParam && hasPageSizeParam))
                throw JsonApiException.CreateForParameterError("Page number missing",
                    string.Format("In order for paging to work properly, if either {0} or {1} is set, both must be.",
                        PageNumberQueryParam, PageSizeQueryParam), PageSizeQueryParam);

            if (pageNumber < 0)
                throw JsonApiException.CreateForParameterError("Page number out of bounds",
                    "Page number must not be negative.", PageNumberQueryParam);

            if (pageSize <= 0)
                throw JsonApiException.CreateForParameterError("Page size out of bounds",
                    "Page size must be greater than or equal to 1.", PageSizeQueryParam);

            if (_maxPageSize != null && pageSize > _maxPageSize.Value)
                pageSize = _maxPageSize.Value;

            var skip = pageNumber * pageSize;
            return new DefaultPaginationTransformResult<T>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                PagedQuery = query.Skip(skip).Take(pageSize),
                PaginationWasApplied = true
            };
        }
    }
}
