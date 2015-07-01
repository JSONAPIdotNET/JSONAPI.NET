using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.ActionFilters;
using JSONAPI.Http;

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

        public async Task<IResourceCollectionDocument> BuildDocument<T>(IQueryable<T> query, HttpRequestMessage request, CancellationToken cancellationToken)
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

            var linkBaseUrl = _baseUrlService.GetBaseUrl(request);

            var results = await _enumerationTransformer.Enumerate(query, cancellationToken);
            return _resourceCollectionDocumentBuilder.BuildDocument(results, linkBaseUrl, null, null);
        }
    }
}
