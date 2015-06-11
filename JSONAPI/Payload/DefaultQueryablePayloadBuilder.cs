using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.ActionFilters;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Provides a default implementation of an IQueryablePayloadBuilder
    /// </summary>
    public class DefaultQueryablePayloadBuilder : IQueryablePayloadBuilder
    {
        private readonly IQueryableEnumerationTransformer _enumerationTransformer;
        private readonly IQueryableFilteringTransformer _filteringTransformer;
        private readonly IQueryableSortingTransformer _sortingTransformer;
        private readonly IQueryablePaginationTransformer _paginationTransformer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerationTransformer"></param>
        /// <param name="filteringTransformer"></param>
        /// <param name="sortingTransformer"></param>
        /// <param name="paginationTransformer"></param>
        public DefaultQueryablePayloadBuilder(
            IQueryableEnumerationTransformer enumerationTransformer,
            IQueryableFilteringTransformer filteringTransformer = null,
            IQueryableSortingTransformer sortingTransformer = null,
            IQueryablePaginationTransformer paginationTransformer = null)
        {
            _enumerationTransformer = enumerationTransformer;
            _filteringTransformer = filteringTransformer;
            _sortingTransformer = sortingTransformer;
            _paginationTransformer = paginationTransformer;
        }

        public async Task<IPayload> BuildPayload<T>(IQueryable<T> query, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_filteringTransformer != null)
                query = _filteringTransformer.Filter(query, request);

            if (_sortingTransformer != null)
                query = _sortingTransformer.Sort(query, request);

            if (_paginationTransformer != null)
            {
                var paginationResults = _paginationTransformer.ApplyPagination(query, request);
                query = paginationResults.PagedQuery;
            }

            var results = await _enumerationTransformer.Enumerate(query, cancellationToken);

            return new Payload
            {
                PrimaryData = results
            };
        }
    }
}
