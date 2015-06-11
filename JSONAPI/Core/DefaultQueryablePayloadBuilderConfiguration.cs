using JSONAPI.ActionFilters;
using JSONAPI.Payload;

namespace JSONAPI.Core
{
    /// <summary>
    /// Provides a fluent configuration system for DefaultQueryablePayloadBuilder
    /// </summary>
    public sealed class DefaultQueryablePayloadBuilderConfiguration
    {
        private bool _enableFiltering;
        private bool _enableSorting;
        private bool _enablePagination;
        private IQueryableFilteringTransformer _filteringTransformer;
        private IQueryableSortingTransformer _sortingTransformer;
        private IQueryablePaginationTransformer _paginationTransformer;
        private IQueryableEnumerationTransformer _enumerationTransformer;

        internal DefaultQueryablePayloadBuilderConfiguration()
        {
            _enableFiltering = true;
            _enableSorting = true;
            _enablePagination = true;
        }

        /// <summary>
        /// Disables filtering of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public DefaultQueryablePayloadBuilderConfiguration DisableFiltering()
        {
            _enableFiltering = false;
            return this;
        }

        /// <summary>
        /// Disables sorting of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public DefaultQueryablePayloadBuilderConfiguration DisableSorting()
        {
            _enableSorting = false;
            return this;
        }

        /// <summary>
        /// Disables pagination of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public DefaultQueryablePayloadBuilderConfiguration DisablePagination()
        {
            _enablePagination = false;
            return this;
        }

        /// <summary>
        /// Specifies a filtering transformer to use for filtering IQueryable response payloads.
        /// </summary>
        /// <param name="filteringTransformer">The filtering transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public DefaultQueryablePayloadBuilderConfiguration FilterWith(IQueryableFilteringTransformer filteringTransformer)
        {
            _filteringTransformer = filteringTransformer;
            return this;
        }

        /// <summary>
        /// Specifies a sorting transformer to use for sorting IQueryable response payloads.
        /// </summary>
        /// <param name="sortingTransformer">The sorting transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public DefaultQueryablePayloadBuilderConfiguration SortWith(IQueryableSortingTransformer sortingTransformer)
        {
            _sortingTransformer = sortingTransformer;
            return this;
        }

        /// <summary>
        /// Specifies a pagination transformer to use for paging IQueryable response payloads.
        /// </summary>
        /// <param name="paginationTransformer">The pagination transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public DefaultQueryablePayloadBuilderConfiguration PageWith(IQueryablePaginationTransformer paginationTransformer)
        {
            _paginationTransformer = paginationTransformer;
            return this;
        }

        /// <summary>
        /// Specifies an enumeration transformer to use for enumerating IQueryable response payloads.
        /// </summary>
        /// <param name="enumerationTransformer">The enumeration transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public DefaultQueryablePayloadBuilderConfiguration EnumerateQueriesWith(IQueryableEnumerationTransformer enumerationTransformer)
        {
            _enumerationTransformer = enumerationTransformer;
            return this;
        }

        internal DefaultQueryablePayloadBuilder GetBuilder(IModelManager modelManager)
        {
            IQueryableFilteringTransformer filteringTransformer = null;
            if (_enableFiltering)
                filteringTransformer = _filteringTransformer ?? new DefaultFilteringTransformer(modelManager);

            IQueryableSortingTransformer sortingTransformer = null;
            if (_enableSorting)
                sortingTransformer = _sortingTransformer ?? new DefaultSortingTransformer(modelManager);

            IQueryablePaginationTransformer paginationTransformer = null;
            if (_enablePagination)
                paginationTransformer =
                    _paginationTransformer ?? new DefaultPaginationTransformer("page.number", "page.size");

            IQueryableEnumerationTransformer enumerationTransformer =
                _enumerationTransformer ?? new SynchronousEnumerationTransformer();

            return new DefaultQueryablePayloadBuilder(enumerationTransformer, filteringTransformer, sortingTransformer, paginationTransformer);
        }
    }
}
