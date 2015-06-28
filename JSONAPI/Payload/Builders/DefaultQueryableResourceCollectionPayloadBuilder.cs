using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.ActionFilters;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Provides a default implementation of an IQueryablePayloadBuilder
    /// </summary>
    public class DefaultQueryableResourceCollectionPayloadBuilder : IQueryableResourceCollectionPayloadBuilder
    {
        private readonly IResourceCollectionPayloadBuilder _resourceCollectionPayloadBuilder;
        private readonly IQueryableEnumerationTransformer _enumerationTransformer;
        private readonly IQueryableFilteringTransformer _filteringTransformer;
        private readonly IQueryableSortingTransformer _sortingTransformer;
        private readonly IQueryablePaginationTransformer _paginationTransformer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceCollectionPayloadBuilder"></param>
        /// <param name="enumerationTransformer"></param>
        /// <param name="filteringTransformer"></param>
        /// <param name="sortingTransformer"></param>
        /// <param name="paginationTransformer"></param>
        public DefaultQueryableResourceCollectionPayloadBuilder(
            IResourceCollectionPayloadBuilder resourceCollectionPayloadBuilder,
            IQueryableEnumerationTransformer enumerationTransformer,
            IQueryableFilteringTransformer filteringTransformer,
            IQueryableSortingTransformer sortingTransformer,
            IQueryablePaginationTransformer paginationTransformer)
        {
            _resourceCollectionPayloadBuilder = resourceCollectionPayloadBuilder;
            _enumerationTransformer = enumerationTransformer;
            _filteringTransformer = filteringTransformer;
            _sortingTransformer = sortingTransformer;
            _paginationTransformer = paginationTransformer;
        }

        public async Task<IResourceCollectionPayload> BuildPayload<T>(IQueryable<T> query, HttpRequestMessage request, CancellationToken cancellationToken)
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

            var linkBaseUrl = new Uri(request.RequestUri.AbsoluteUri.Replace(request.RequestUri.PathAndQuery, String.Empty)).ToString();

            var results = await _enumerationTransformer.Enumerate(query, cancellationToken);
            return _resourceCollectionPayloadBuilder.BuildPayload(results, linkBaseUrl, null);
        }
    }
}
