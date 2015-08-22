using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Http;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Provides a default implementation of an IQueryableResourceCollectionDocumentBuilder
    /// </summary>
    public class DefaultQueryableResourceCollectionDocumentBuilder : IQueryableResourceCollectionDocumentBuilder
    {
        private readonly IResourceCollectionDocumentBuilder _resourceCollectionDocumentBuilder;
        private readonly IQueryableEnumerationTransformer _enumerationTransformer;
        private readonly IQueryableFilteringTransformer _filteringTransformer;
        private readonly IQueryableSortingTransformer _sortingTransformer;
        private readonly IQueryablePaginationTransformer _paginationTransformer;
        private readonly IBaseUrlService _baseUrlService;

        /// <summary>
        /// Creates a new DefaultQueryableResourceCollectionDocumentBuilder
        /// </summary>
        public DefaultQueryableResourceCollectionDocumentBuilder(
            IResourceCollectionDocumentBuilder resourceCollectionDocumentBuilder,
            IQueryableEnumerationTransformer enumerationTransformer,
            IQueryableFilteringTransformer filteringTransformer,
            IQueryableSortingTransformer sortingTransformer,
            IQueryablePaginationTransformer paginationTransformer,
            IBaseUrlService baseUrlService)
        {
            _resourceCollectionDocumentBuilder = resourceCollectionDocumentBuilder;
            _enumerationTransformer = enumerationTransformer;
            _filteringTransformer = filteringTransformer;
            _sortingTransformer = sortingTransformer;
            _paginationTransformer = paginationTransformer;
            _baseUrlService = baseUrlService;
        }

        public async Task<IResourceCollectionDocument> BuildDocument<T>(IQueryable<T> query, HttpRequestMessage request, CancellationToken cancellationToken,
            string[] includes = null)
        {
            var filteredQuery = _filteringTransformer.Filter(query, request);
            var sortedQuery = _sortingTransformer.Sort(filteredQuery, request);

            var paginationResults = _paginationTransformer.ApplyPagination(sortedQuery, request);
            var paginatedQuery = paginationResults.PagedQuery;

            var linkBaseUrl = _baseUrlService.GetBaseUrl(request);

            var results = await _enumerationTransformer.Enumerate(paginatedQuery, cancellationToken);
            var metadata = await GetDocumentMetadata(query, filteredQuery, sortedQuery, paginationResults, cancellationToken);
            return _resourceCollectionDocumentBuilder.BuildDocument(results, linkBaseUrl, includes, metadata);
        }

        /// <summary>
        /// Returns the metadata that should be sent with this document.
        /// </summary>
        protected virtual Task<IMetadata> GetDocumentMetadata<T>(IQueryable<T> originalQuery, IQueryable<T> filteredQuery, IOrderedQueryable<T> sortedQuery,
            IPaginationTransformResult<T> paginationResult, CancellationToken cancellationToken)
        {
            return Task.FromResult((IMetadata)null);
        }
    }
}
